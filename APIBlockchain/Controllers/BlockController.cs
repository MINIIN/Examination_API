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
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using System.Text;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("")]
    //[Route("api/[controller]")]
    public class BlockController : ControllerBase
    {
        private readonly ILogger<GetBlockController> _logger;

        public BlockController(ILogger<GetBlockController> logger)
        {
            _logger = logger;
        }

        [Route("GetAccountBalance")]
        [HttpPost]
        public async Task<decimal> GetAccountBalance(string address, string account)
        {
            var web3 = new Web3(address);

            var balance = await web3.Eth.GetBalance.SendRequestAsync(account);
            var etherAmount = Web3.Convert.FromWei(balance.Value);

            var networkId = await web3.Net.Version.SendRequestAsync();
            var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var latestBlock = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(latestBlockNumber);

            return decimal.Parse(etherAmount.ToString());
        }

        [Route("GetBlockNumber")]
        [HttpPost]
        public async Task<int> GetBlockNumber(string address)
        {

            var web3 = new Web3(address);

            var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

            return int.Parse(latestBlockNumber.ToString());
        }

        [Route("GetTxAddress")]
        [HttpPost]
        public async Task<string> GetTxAddress(string address)
        {

            //string address = "HTTP://127.0.0.1:7545";
            //string account = "0xc3015F16De040f79af850bF9b3e97Cb97cFE3A7C";
            //string private_key = "315f5004eb9e7209ffefcea1840e82cec17be1ed9e017a43f4f29b85ce71858d";


            var web3 = new Web3(address);

            var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var txnReceipt = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(latestBlockNumber);
            //var txnReceipt = await web3.Eth.TransactionManager.(txnInput);

            var txAddress = txnReceipt.TransactionHashes[0].ToString();


            return txAddress;
        }

        [Route("AddData")]
        [HttpPost]
        public async Task<string> AddData(string address, string account, string accountTo, string Data)
        {
            var web3 = new Web3(address);

            web3.TransactionReceiptPolling.SetPollingRetryIntervalInMilliseconds(200);
            web3.TransactionManager.UseLegacyAsDefault = true;

            var txnInput = new TransactionInput();
            txnInput.From = account;
            txnInput.To = accountTo;
            txnInput.Data = Data.ToHexUTF8();
            txnInput.Gas = new HexBigInteger(900000);
            var txnReceipt = await web3.Eth.TransactionManager.SendTransactionAndWaitForReceiptAsync(txnInput);

            return txnReceipt.TransactionHash;
        }

        [Route("GetData")]
        [HttpPost]
        public async Task<string> GetData(string address, string txHash)
        {
            var web3 = new Web3(address);

            var txn = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);
            string txHashHex = txn.Input;
            string input = Encoding.ASCII.GetString(txHashHex.HexToByteArray());

            return input;
        }


    }
}