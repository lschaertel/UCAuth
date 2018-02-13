﻿using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace Neo.SmartContract
{
    public class Contract1 : Framework.SmartContract
    {
        public static object Main(string operation, params object[] args)
        {
            switch (operation)
            {
                case "query":
                    return Query((string)args[0]);
                case "register":
                    return Register((string)args[0], (byte[])args[2]);
                case "transfer":
                    return Transfer((string)args[0], (byte[])args[2]);
                case "delete":
                    return Delete((string)args[0]);
                case "balance":
                    return Balance((byte[])args[1], (byte[])args[2]);
                default:
                    return false;
            }
        }
        
        private static Boolean Balance(byte[] publicHash, byte[] owner)
        {
            Account acc = Blockchain.GetAccount(publicHash);
            byte[] value = Storage.Get(Storage.CurrentContext, "tesasdasdt");
            if (value != null) return true;
            return false;
            
            //Asset ass = Blockchain.GetAsset(owner);
            //return ass.Amount;
            //return Storage.Get(Storage.CurrentContext, publicHash.Concat(owner)).AsBigInteger();
        }
        private static byte[] Query(string domain)
        {
            return Storage.Get(Storage.CurrentContext, domain);
        }
        private static bool Register(string domain, byte[] owner)
        {
            if (!Runtime.CheckWitness(owner)) return false;
            byte[] value = Storage.Get(Storage.CurrentContext, domain);
            if (value != null) return false;
            Storage.Put(Storage.CurrentContext, domain, owner);
            return true;
        }
        private static bool Transfer(string domain, byte[] to)
        {
            if (!Runtime.CheckWitness(to)) return false;
            byte[] from = Storage.Get(Storage.CurrentContext, domain);
            if (from == null) return false;
            if (!Runtime.CheckWitness(from)) return false;
            Storage.Put(Storage.CurrentContext, domain, to);
            return true;
        }
        private static bool Delete(string domain)
        {
            byte[] owner = Storage.Get(Storage.CurrentContext, domain);
            if (owner == null) return false;
            if (!Runtime.CheckWitness(owner)) return false;
            Storage.Delete(Storage.CurrentContext, domain);
            return true;
        }
    }
}
