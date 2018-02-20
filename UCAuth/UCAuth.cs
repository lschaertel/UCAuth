using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace Neo.SmartContract
{
    public class UCAuth : Framework.SmartContract
    {
        public static object Main(string operation, params object[] args)
        {
            switch (operation)
            {
                case "setStorage":
                    return setStorage((string)args[0], (object[])args[1]);
                case "listStorage":
                    return listStorage((string)args[0]);
                case "getStorage":
                    return getStorage((string)args[0], (object[])args[1]);
                case "deleteStorage":
                    return deleteStorage((string)args[0], (object[])args[1]);
                default:    
                    return false;
            }
        }
        //Adds a new Domain-Credencial to the given public key and relevant parameters
        private static Boolean setStorage(string pubKey, object[] args)
        {
            byte[] ownerAddress = pubKey.AsByteArray();
            byte[] domain = (byte[])args[0];
            byte[] username = (byte[])args[1];
            byte[] password = (byte[])args[2];
            //byte[] encryptionType = (byte[])args[3]; //ToDo: enable encryption - EncryptionTypes: None/Password/Privatekey (Hash?))
            //byte[] encryption = (byte[])args[4];

            byte[] amountDomains = Storage.Get(Storage.CurrentContext, ownerAddress);
                    //return rearrangeStorage((byte[])args[0], (byte[])args[1]);
            if (amountDomains == null)
            {
                Storage.Put(Storage.CurrentContext, ownerAddress, 49);
                amountDomains = Storage.Get(Storage.CurrentContext, ownerAddress);
            }
            else
            {
                amountDomains = (amountDomains.AsBigInteger() + 1).AsByteArray();
            }
            Runtime.Notify("ownerAddress: ", ownerAddress);
            Runtime.Notify("domain: ", domain);
            Runtime.Notify("amountDomains: ", amountDomains);

            byte[] domainNo = ownerAddress.Concat(amountDomains);
            byte[] domainName = ownerAddress.Concat(domain);
            
            if (!checkExisting(domainName))
            {
                byte[] domainUsername = domainNo.Concat("username".AsByteArray());
                byte[] domainPassword = domainNo.Concat("password".AsByteArray());
                byte[] encryptionType = domainNo.Concat("encryptiontype".AsByteArray());
                byte[] encryptionKey = domainNo.Concat("encryptionkey".AsByteArray());

                Storage.Put(Storage.CurrentContext, domainNo, domain);
                Storage.Put(Storage.CurrentContext, domainName, domainNo);
                Storage.Put(Storage.CurrentContext, domainUsername, username);
                Storage.Put(Storage.CurrentContext, domainPassword, password);
                Storage.Put(Storage.CurrentContext, ownerAddress, amountDomains);
                
                Runtime.Notify("domainNo: ", domainNo);
                Runtime.Notify("domainName: ", domainName);
                Runtime.Notify("domainUsername: ", domainUsername);
                
                return true;
            }
            return false;
        }
        private static object encryptData()
        {
            string test;
            
            return false;
        }
        private static object decryptData()
        {

            return false;
        }

        //Returns a container with every added domain to the given PublicKey
        private static object[] listStorage(string pubKey)
        {
            int counterResults = 0;
            byte[] ownerAddress = pubKey.AsByteArray();
            BigInteger amountDomains = Storage.Get(Storage.CurrentContext, ownerAddress).AsBigInteger();
            object[] results = new object[(int)amountDomains - 48];

            Runtime.Notify("amountDomains: ", amountDomains);

            for (BigInteger i = 49; i <= amountDomains;)
            {
                byte[] domainNo = ownerAddress.Concat(i.AsByteArray());
                results[counterResults] = Storage.Get(Storage.CurrentContext, domainNo);

                counterResults++;
                i = i + 1;
            }

            return results;
        }
        //Checks if there is an existing entry on that given domainName → [PublicKey]+[domain]
        private static Boolean checkExisting(byte[] domainName)
        {
            var result = Storage.Get(Storage.CurrentContext, domainName);
            if (result == null) return false;
            return true;
        }
        //Returns a container with the personell data to the given PublicKey and domain
        private static object getStorage(string pubKey, object[] args)
        {
            byte[] ownerAddress = pubKey.AsByteArray();
            byte[] domain = (byte[])args[0];
            byte[] domainNo = Storage.Get(Storage.CurrentContext, ownerAddress.Concat(domain));
            if (domainNo == null) return false;

            object[] results = new object[3];
            results[0] = domain;
            results[1] = Storage.Get(Storage.CurrentContext, domainNo.Concat("username".AsByteArray()));
            results[2] = Storage.Get(Storage.CurrentContext, domainNo.Concat("password".AsByteArray()));

            return results;
        }
        private static Boolean deleteStorage(string pubKey, object[] args)
        {
            byte[] ownerAddress = pubKey.AsByteArray();
            byte[] domain = (byte[])args[0];
            byte[] domainNo = Storage.Get(Storage.CurrentContext, ownerAddress.Concat(domain));
            Runtime.Notify("deletedDomain:", domain);
            if (domainNo == null) return false;

            byte[] domainUsername = domainNo.Concat("username".AsByteArray());
            byte[] domainPassword = domainNo.Concat("password".AsByteArray());
            byte[] encryptionType = domainNo.Concat("encryptiontype".AsByteArray());
            byte[] encryptionKey = domainNo.Concat("encryptionkey".AsByteArray());

            Storage.Delete(Storage.CurrentContext, domainNo);
            Storage.Delete(Storage.CurrentContext, ownerAddress.Concat(domain));
            Storage.Delete(Storage.CurrentContext, domainUsername);
            Storage.Delete(Storage.CurrentContext, domainPassword);
            //Storage.Delete(Storage.CurrentContext, encryptionType);
            //Storage.Delete(Storage.CurrentContext, encryptionKey);

            Storage.Put(Storage.CurrentContext, ownerAddress, (Storage.Get(Storage.CurrentContext, ownerAddress).AsBigInteger() - 1));

            rearrangeStorage(ownerAddress, domainNo, domain);

            return true;
        }
        //ToDo Rearrange correctly 
        private static Boolean rearrangeStorage(byte[] ownerAddress, byte[] deletedDomainNo, byte[] domainDeleted)
        {
            var amountDomains = Storage.Get(Storage.CurrentContext, ownerAddress).AsBigInteger();
            var deletedNo = (BigInteger)deletedDomainNo[deletedDomainNo.Length - 1];
            byte[] newDomainNo = ownerAddress.Concat(deletedNo.AsByteArray());
            Runtime.Notify("Deleted Storage Location: ", newDomainNo);
            
            if(amountDomains >= deletedNo)
            {
                byte[] domainNo = ownerAddress.Concat(((BigInteger)amountDomains + 1).AsByteArray());
                byte[] domain = Storage.Get(Storage.CurrentContext, domainNo);
                byte[] domainUsername = Storage.Get(Storage.CurrentContext, domainNo.Concat("username".AsByteArray()));
                byte[] domainPassword = Storage.Get(Storage.CurrentContext, domainNo.Concat("password".AsByteArray()));
                byte[] encryptionType = Storage.Get(Storage.CurrentContext, domainNo.Concat("encryptiontype".AsByteArray()));
                byte[] encryptionKey = Storage.Get(Storage.CurrentContext, domainNo.Concat("encryptionkey".AsByteArray()));

                byte[] domainUsernameDeleted = deletedDomainNo.Concat("username".AsByteArray());
                byte[] domainPasswordDeleted = deletedDomainNo.Concat("password".AsByteArray());
                byte[] encryptionTypeDeleted = deletedDomainNo.Concat("encryptiontype".AsByteArray());
                byte[] encryptionKeyDeleted = deletedDomainNo.Concat("encryptionkey".AsByteArray());

                Storage.Put(Storage.CurrentContext, deletedDomainNo, domain);
                Storage.Put(Storage.CurrentContext, ownerAddress.Concat(domain), domainNo);
                Storage.Put(Storage.CurrentContext, domainUsernameDeleted, domainUsername);
                Storage.Put(Storage.CurrentContext, domainPasswordDeleted, domainPassword);
                Storage.Put(Storage.CurrentContext, encryptionTypeDeleted, encryptionType);
                Storage.Put(Storage.CurrentContext, encryptionKeyDeleted, encryptionKey);
            }
            return true;
        }
    }
}
