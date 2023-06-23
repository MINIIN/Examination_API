using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security;
using WebApplication3.Models;
using WebApplication3.Models.Data;
using WebApplication3.Controllers;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Text;
using WebApplication3.Class;
using NBitcoin.Secp256k1;

namespace WebApplication3.Controllers
{
    [Route("api/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        static string _address = "HTTP://127.0.0.1:7545";
        static string _account = "0x00f86E5F959bbe635f81151B7c19558146b577Ae";
        static string _accountTo = "0x12890d2cce102216644c59daE5baed380d84830c";


        [Route("GetAllUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllUser()
        {

            var context = new DataContext();
            var user = context.User.ToList();

            return Ok(user);
        }

        [Route("AddUser")]
        [HttpPost]
        public async Task<IActionResult> AddUser(string name, string email, string password, int permisson, string studentId = "")
        {

            var context = new DataContext();

            User user_last = context.User.OrderBy(c => c.ID).ToList().LastOrDefault();
            int id = user_last.ID.HasValue ? user_last.ID.Value : 0;

            User user = new User();

            user.ID = id + 1;
            user.Name  = name;
            user.Email = email;
            user.Password = password;
            user.Permisson = permisson;
            user.StudentId = studentId;

            context.User.Add(user);
            context.SaveChanges();

            return Ok(user);
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> login(string email, string password)
        {
            //bool login = false;
            var context = new DataContext();

            User user = context.User.Where(c => c.Email == email).FirstOrDefault();
            User retult = new User();
            if(user != null)
            {
                if(user.Password == password) 
                {
                    retult = user;

                    return Ok(retult);
                }
            }


            return Unauthorized(retult);
        }

        [Route("AddSubject")]
        [HttpPost]
        public async Task<IActionResult> AddSubject(string subjectName, int teacherId)
        {

            var context = new DataContext();
            Subject subjects = context.Subject.OrderBy(c => c.ID).ToList().LastOrDefault();

            int id = 0;
            if (subjects != null)
            {
                id = subjects.ID.HasValue ? subjects.ID.Value : 0;
            }

            Subject subject = new Subject();

            subject.ID = id + 1;
            subject.Name = subjectName;
            subject.TeacherId = teacherId;


            context.Subject.Add(subject);
            context.SaveChanges();

            return Ok(subject);
        }

        [Route("AddScore")]
        [HttpPost]
        public async Task<IActionResult> AddScore(int idUser, int id_subject, string score)
        {

            var web3 = new Web3(_address);

            web3.TransactionReceiptPolling.SetPollingRetryIntervalInMilliseconds(200);
            web3.TransactionManager.UseLegacyAsDefault = true;

            var txnInput = new TransactionInput();
            txnInput.From = _account;
            txnInput.To = _accountTo;
            txnInput.Data = score.ToHexUTF8();
            txnInput.Gas = new HexBigInteger(900000);
            var txnReceipt = await web3.Eth.TransactionManager.SendTransactionAndWaitForReceiptAsync(txnInput);


            var context = new DataContext();
            Score_user scoreUsers = context.Score_user.OrderBy(c => c.ID).ToList().LastOrDefault();

            int id = 0;
            if (scoreUsers != null)
            {
                id = scoreUsers.ID.HasValue ? scoreUsers.ID.Value : 0;
            }

            Score_user scoreUser = new Score_user();

            scoreUser.ID = id + 1;
            scoreUser.Id_user = idUser;
            scoreUser.Id_subject = id_subject;
            scoreUser.score = txnReceipt.TransactionHash.ToString();
            scoreUser.user_name = string.Empty;


            context.Score_user.Add(scoreUser);
            context.SaveChanges();

            return Ok(scoreUser);
        }

        [Route("GetRegisterSubject")]
        [HttpPost]
        public async Task<IActionResult> RegisterSubject(int userid)
        {

            var context = new DataContext();
            List<Subject> subjects = context.Subject.ToList();
            List<Register_subject> addSubjects = context.Register_subject.Where(c => c.userId == userid).ToList();

            var result = (
                (from s in subjects
                join a in addSubjects
                on s.ID equals a.subjectId into aa
                from add in aa.DefaultIfEmpty()
                select new 
                {
                    ID = s.ID,
                    Name= s.Name,
                    TeacherId =s.TeacherId,
                    ID_re = add?.id ?? 0,

                })
                ).Where(c => c.ID_re == 0);



            return Ok(result);
        }

        [Route("RegisterSubject")]
        [HttpPost]
        public async Task<IActionResult> RegisterSubject(int userid, int subjectid)
        {
            var context = new DataContext();
            Register_subject addSubjects = context.Register_subject.OrderBy(c => c.id).ToList().LastOrDefault();
            Register_subject addSubject = new Register_subject();

            int id = 0;
            if (addSubjects != null)
            {
                id = addSubjects.id.HasValue ? addSubjects.id.Value : 0;
            }

            addSubject.id = id+1;
            addSubject.userId = userid;
            addSubject.subjectId = subjectid;



            context.Register_subject.Add(addSubject);
            context.SaveChanges();


            return Ok(addSubject);
        }


        [Route("GetScoreByUser")]
        [HttpPost]
        public async Task<IActionResult> GetScore(int idUser)
        {

            var context = new DataContext();
            List<ScoreUser> scores= new List<ScoreUser>();
            List<Score_user> scoreUsers = context.Score_user.Where(c => c.Id_user == idUser).ToList();
            //Score_user scoreUser = new Score_user();

            if (scoreUsers != null)
            {
                foreach (Score_user scoreUser in scoreUsers)
                {
                    // get score
                    ScoreUser score = new ScoreUser();
                    var web3 = new Web3(_address);

                    var txn = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(scoreUser.score);

                    string txHashHex = txn.Input;
                    string input = Encoding.ASCII.GetString(txHashHex.HexToByteArray());

                    scoreUser.score = input;

                    //get name
                    User user = context.User.Where(c => c.ID == scoreUser.Id_user).ToList().LastOrDefault();
                    Subject subject = context.Subject.Where(c => c.ID == scoreUser.Id_subject).ToList().LastOrDefault();
                    //scoreUser.user_name = user.Name;

                    score.ID = scoreUser.ID;
                    score.Id_user = scoreUser.Id_user;
                    score.Id_subject = scoreUser.Id_subject;
                    score.score = scoreUser.score;
                    score.user_name = user.Name;
                    score.subjectName = subject.Name;

                    scores.Add(score);
                }

            }         

            return Ok(scores);
        }

        [Route("GetScoreByTeacher")]
        [HttpPost]
        public async Task<IActionResult> GetScoreByTeacher(int idUser)
        {

            var context = new DataContext();
            List<Subject> subjects = context.Subject.Where(c => c.TeacherId == idUser).ToList();
            List<Score_user> Scores = new List<Score_user>();

            if (subjects != null)
            {
                foreach(Subject subject in subjects)
                {
                    List<Score_user> scoreUsers = context.Score_user.Where(c => c.Id_subject == subject.ID).ToList();

                    foreach(Score_user scoreUser in scoreUsers)
                    {
                        //get name
                        User user = context.User.Where(c => c.ID == scoreUser.Id_user).ToList().LastOrDefault();
                        scoreUser.user_name = user.Name;

                        //get score
                        var web3 = new Web3(_address);

                        var txn = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(scoreUser.score);
                        string txHashHex = txn.Input;
                        string input = Encoding.ASCII.GetString(txHashHex.HexToByteArray());
                        scoreUser.score = input;

                        Scores.Add(scoreUser);
                    }
                    
                   
                }
            }

            return Ok(Scores);
        }

        [Route("GetStudenByTeacher")]
        [HttpPost]
        public async Task<IActionResult> GetStudenByTeacher(int idUser)
        {

            var context = new DataContext();
            List<Subject> subjects = context.Subject.Where(c => c.TeacherId == idUser).ToList();
            List<Score_user> Scores = new List<Score_user>();
            List<AddScore> addScores = new List<AddScore>();

            if (subjects != null)
            {
                foreach (Subject subject in subjects)
                {
                    List<Register_subject> registerSubjects = context.Register_subject.Where(c => c.subjectId == subject.ID).ToList();

                    foreach (Register_subject registerSubject in registerSubjects)
                    {
                        Score_user scores = context.Score_user.Where(c => c.Id_subject== registerSubject.subjectId && c.Id_user == registerSubject.userId).ToList().LastOrDefault();

                        if(scores == null)
                        {
                            AddScore addScore = new AddScore();
                            //get name
                            User user = context.User.Where(c => c.ID == registerSubject.userId).ToList().LastOrDefault();
                            //scores.user_name = user.Name;
                            addScore.id = user.ID;
                            addScore.Name = user.Name;
                            addScore.StudentId = user.StudentId;
                            addScore.subjectId = subject.ID;

                            addScores.Add(addScore);
                        }    
                        
                    }


                }
            }

            return Ok(addScores);
        }

        [Route("GetStudy")]
        [HttpGet]
        public async Task<IActionResult> GetStudy()
        {

            var context = new DataContext();
            List<User> users = context.User.Where(c => c.Permisson == 3).ToList();

            return Ok(users);
        }

        [Route("GetTeacher")]
        [HttpGet]
        public async Task<IActionResult> GetTeacher()
        {

            var context = new DataContext();
            List<User> users = context.User.Where(c => c.Permisson == 2).ToList();

            return Ok(users);
        }

        [Route("GetSubject")]
        [HttpGet]
        public async Task<IActionResult> GetSubject()
        {

            var context = new DataContext();
            List<Subject> subjects = context.Subject.ToList();

            return Ok(subjects);
        }


        [Route("GetUser")]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {

            var context = new DataContext();
            List<DashbordUser> users = new List<DashbordUser>();

            DashbordUser user = new DashbordUser();
            int userTester = context.User.Where(c => c.Permisson == 2).Count();

            user.Name = "Tester";
            user.Count = userTester;

            users.Add(user);

            DashbordUser user1 = new DashbordUser();
            int userExamination = context.User.Where(c => c.Permisson == 3).Count();

            user1.Name = "Examination";
            user1.Count = userExamination;

            users.Add(user1);

            return Ok(users);
        }

        [Route("GetSubject1")]
        [HttpGet]
        public async Task<IActionResult> GetSubject1()
        {

            var context = new DataContext();
            List<DashbordUser> users = new List<DashbordUser>();

            List<Subject> subjects = context.Subject.ToList();

            foreach(var subject in subjects)
            {
                DashbordUser dashbord = new DashbordUser();
                int count = context.Register_subject.Where(c => c.subjectId == subject.ID).Count();

                dashbord.Name = subject.Name;
                dashbord.Count = count;

                users.Add(dashbord);
            }


            return Ok(users);
        }



    }
}
