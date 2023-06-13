using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SSI
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public IList<Transaction> Transactions { get; set; }
        //public int Nonce { get; set; } = 0;
        public string Validator { get; set; }
        public string MerkleRoot { get; set; }

        public Block(DateTime timeStamp, string previousHash, IList<Transaction> transactions, string validator)
        {
            Index = 0;
            TimeStamp = timeStamp;
            PreviousHash = previousHash;
            Transactions = transactions;
            Validator = validator;
            MerkleRoot = CalculateMerkleRoot();
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes($"{TimeStamp}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Transactions)}-{Validator}-{MerkleRoot}");
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(outputBytes);
        }

        private string CalculateMerkleRoot()
        {
            List<string> transactionHashes = new List<string>();

            foreach (Transaction transaction in Transactions)
            {
                string transactionData = $"{transaction.FromAddress}{transaction.ToAddress}{transaction.Amount}";
                string transactionHash = CalculateTransactionHash(transactionData);
                transactionHashes.Add(transactionHash);
            }

            while (transactionHashes.Count > 1)
            {
                List<string> newHashes = new List<string>();

                for (int i = 0; i < transactionHashes.Count; i += 2)
                {
                    string combinedHash = transactionHashes[i] + transactionHashes[i + 1];
                    string newHash = CalculateTransactionHash(combinedHash);
                    newHashes.Add(newHash);
                }

                if (transactionHashes.Count % 2 != 0)
                    newHashes.Add(transactionHashes[^1]);

                transactionHashes = newHashes;
            }
            if(transactionHashes.Count > 0)
            {
                return transactionHashes[0];
            }
            else {
                return "";
            }
            
        }

        private string CalculateTransactionHash(string transactionData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(transactionData);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }


    }

}
