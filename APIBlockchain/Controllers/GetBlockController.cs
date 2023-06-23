using Microsoft.AspNetCore.Mvc;
//using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using Nethereum.ABI.EIP712;
using System.Globalization;
using Nethereum.BlockchainProcessing.BlockStorage.Repositories;
using System.Threading;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using Nethereum.RLP;
using Newtonsoft.Json;
using System.Drawing;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/")]
    //[Route("api/[controller]")]
    public class GetBlockController : ControllerBase
    {
        private readonly ILogger<GetBlockController> _logger;
        static string _address = "HTTP://127.0.0.1:7545";
        static string _account = "0x00f86E5F959bbe635f81151B7c19558146b577Ae";
        static string _accountTo = "0x12890d2cce102216644c59daE5baed380d84830c";
        //static string _txHash = "0xf3b0cebb275b0b65d95de1d5975a4ce1684660b5e7605878f9ed40549673caec";

        public GetBlockController(ILogger<GetBlockController> logger)
        {
            _logger = logger;
        }

        [Route("GetAddress")]
        [HttpGet]
        public async Task<IActionResult> GetAddress()
        {

            var web3 = new Web3(_address);

            var balance = await web3.Eth.GetBalance.SendRequestAsync(_account);
            var etherAmount = Web3.Convert.FromWei(balance.Value);

            //var networkId = await web3.Net.Version.SendRequestAsync();
            //var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            //var latestBlock = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(latestBlockNumber);

            return Ok(new Item { result = _account });
        }

        [Route("GetAccountBalance")]
        [HttpGet]
        public async Task<IActionResult> GetAccountBalance()
        {

            var web3 = new Web3(_address);

            var balance = await web3.Eth.GetBalance.SendRequestAsync(_account);
            var etherAmount = Web3.Convert.FromWei(balance.Value);

            return Ok(new Item { result = decimal.Parse(etherAmount.ToString()).ToString() });
        }

        [Route("GetBlockNumber")]
        [HttpGet]
        public async Task<IActionResult> GetBlockNumber()
        {

            //string address = "HTTP://127.0.0.1:7545";
            //string account = "0xc3015F16De040f79af850bF9b3e97Cb97cFE3A7C";
            //string private_key = "315f5004eb9e7209ffefcea1840e82cec17be1ed9e017a43f4f29b85ce71858d";


            var web3 = new Web3(_address);

            var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();



            //return Ok(int.Parse(latestBlockNumber.ToString()));
            return Ok(new Item { result = int.Parse(latestBlockNumber.ToString()).ToString() });
        }

        [Route("GetLastTxAddress")]
        [HttpGet]
        public async Task<IActionResult> GetTxAddress()
        {

            //string address = "HTTP://127.0.0.1:7545";
            //string account = "0xc3015F16De040f79af850bF9b3e97Cb97cFE3A7C";
            //string private_key = "315f5004eb9e7209ffefcea1840e82cec17be1ed9e017a43f4f29b85ce71858d";


            var web3 = new Web3(_address);

            var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var txnReceipt = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(latestBlockNumber);
            //var txnReceipt = await web3.Eth.TransactionManager.(txnInput);

            var txAddress = txnReceipt.TransactionHashes[0].ToString();


            //return Ok(txAddress);
            return Ok(new Item { result = txAddress });
        }

        [Route("AddData")]
        [HttpPost]
        public async Task<IActionResult> AddData(string data)
        {

            //string address = "HTTP://127.0.0.1:7545";
            //string account = "0xc3015F16De040f79af850bF9b3e97Cb97cFE3A7C";
            //string accountTo = "0x12890d2cce102216644c59daE5baed380d84830c";
            //string private_key = "315f5004eb9e7209ffefcea1840e82cec17be1ed9e017a43f4f29b85ce71858d";


            var web3 = new Web3(_address);

            web3.TransactionReceiptPolling.SetPollingRetryIntervalInMilliseconds(200);
            web3.TransactionManager.UseLegacyAsDefault = true;

            var txnInput = new TransactionInput();
            txnInput.From = _account;
            txnInput.To = _accountTo;
            txnInput.Data = data.ToHexUTF8();
            txnInput.Gas = new HexBigInteger(900000);

            var txnReceipt = await web3.Eth.TransactionManager.SendTransactionAndWaitForReceiptAsync(txnInput);

            //var txnSigned = await web3.Eth.TransactionManager.SignTransactionAsync(txnInput);
            //var txnHash = TransactionUtils.CalculateTransactionHash(txnSigned);

            //var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();



            //return Ok(txnReceipt.TransactionHash);
            return Ok(new Item { result = txnReceipt.TransactionHash });
        }

        [Route("GetData")]
        [HttpGet]
        public async Task<IActionResult> GetData(string txHash)
        {

            //string address = "HTTP://127.0.0.1:7545";
            //string account = "0xc3015F16De040f79af850bF9b3e97Cb97cFE3A7C";
            //string txHash = "0xf3b0cebb275b0b65d95de1d5975a4ce1684660b5e7605878f9ed40549673caec";
            //string private_key = "315f5004eb9e7209ffefcea1840e82cec17be1ed9e017a43f4f29b85ce71858d";


            var web3 = new Web3(_address);

            //var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            //Console.WriteLine("Current BlockNumber is: " + blockNumber.Value);

            var txn = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);
            string txHashHex = txn.Input;
            string input = Encoding.ASCII.GetString(txHashHex.HexToByteArray());


            //return Ok(input);
            return Ok(new Item { result = input });
        }

    }
}