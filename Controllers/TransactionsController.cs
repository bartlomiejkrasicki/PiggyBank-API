using Microsoft.AspNetCore.Mvc;
using PiggyBank_API.Models;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using Microsoft.AspNetCore.Cors;

namespace PiggyBank_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : Controller
    {
        IFirebaseConfig firebaseConfig = new FirebaseConfig
        {
            AuthSecret = "yQ721PmNtfRRCbxFDklZHJIJClrCW85gHjBswj1F",
            BasePath = "https://piggybank-budget-manager.firebaseio.com/"
        };
        IFirebaseClient client;

        public TransactionsController()
        {
            client = new FireSharp.FirebaseClient(firebaseConfig);
        }

        [HttpGet]
        public ActionResult GetAll()
        {
            FirebaseResponse response = client.Get("Transaction/");
            string transaction = response.Body;
            transaction = transaction.Replace("null,", "");
            IList<Transaction> allTransactions = new List<Transaction>();
            try
            {
                JObject transactionJo = JObject.Parse(transaction);
                foreach (var transactionItem in transactionJo.Children())
                {
                    JToken transactionJt = transactionItem as JToken;
                    IList<Transaction> singleTransaction = transactionJt.Select(p => new Transaction
                    {
                        AddDate = (DateTime)p["AddDate"],
                        Amount = (int)p["Amount"],
                        Category = (string)p["Category"],
                        Id = (int)p["Id"],
                        Type = (string)p["Type"]
                    }).ToList();
                    allTransactions.Add(singleTransaction[0]);
                }
            }
            catch{
                return new ObjectResult(allTransactions);
            }
            return new ObjectResult(allTransactions);
        }

        [HttpGet("{id}", Name = "GetTransaction")]
        public ActionResult GetById(int id)
        {
            FirebaseResponse response = client.Get("Transaction/" + id);
            Transaction transaction = response.ResultAs<Transaction>();
            return new ObjectResult(transaction);
        }

        [HttpPost]
        public ActionResult Create([FromBody] Transaction transaction)
        {
            FirebaseResponse countGetResponse = client.Get("AI");
            if (countGetResponse.Body == "null")
            {
                SetResponse countSet = client.Set("AI/Count", 0);
                countGetResponse = client.Get("AI");
            }

            AutoIncrement countInstance = countGetResponse.ResultAs<AutoIncrement>();
            transaction.Id = countInstance.Count;
            SetResponse transactionResponse = client.Set("Transaction/" + countInstance.Count, transaction);

            countInstance.Count++;
            FirebaseResponse countUpdateResponse = client.Update("AI", countInstance);
            return new ObjectResult(transaction);
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Transaction transaction)
        {
            transaction.Id = id;
            FirebaseResponse transactionResponse = client.Update("Transaction/" + id, transaction);
            if (transactionResponse.Body != "null")
            {
                Transaction updateTransaction = transactionResponse.ResultAs<Transaction>();
            }
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            FirebaseResponse transactionResponse = client.Delete("Transaction/" + id);
            if (transactionResponse.Body != "null")
            {
                Transaction deleteTransaction = transactionResponse.ResultAs<Transaction>();
            }
            return new NoContentResult();
        }

        [HttpGet]
        [Route("getbalance")]
        public ActionResult GetBalance()
        {
            FirebaseResponse response = client.Get("Transaction/");
            string transaction = response.Body;
            transaction = transaction.Replace("null,", "");
            IList<Transaction> allTransactions = new List<Transaction>();
            double balance = 0;
            try
            {
                JObject transactionJo = JObject.Parse(transaction);
                foreach (var transactionItem in transactionJo.Children())
                {
                    JToken transactionJt = transactionItem as JToken;
                    IList<Transaction> singleTransaction = transactionJt.Select(p => new Transaction
                    {
                        Amount = (int)p["Amount"],
                        Type = (string)p["Type"]
                    }).ToList();
                    if (singleTransaction[0].Type.Equals("Income")){
                        balance += singleTransaction[0].Amount;
                    } else {
                        balance -= singleTransaction[0].Amount;
                    }
                }
            }
            catch{
                return new ObjectResult(balance);
            }
            return new ObjectResult(balance);
        }
    }
}