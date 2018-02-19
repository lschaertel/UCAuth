using Neo.SmartContract.Framework;
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
                case "setStorage":
                    return setStorage((string)args[0], (object[])args[1]);
                case "getStorage":
                    return getStorage((string)args[0], (object[])args[1]);
                case "register":
                    return Register((string)args[0], (byte[])args[2]);
                default:
                    return false;
            }
        }

        private static Boolean setStorage(string pubKey, object[] args)
        {
            byte[] ownerAddress = pubKey.AsByteArray();
            byte[] domain = (byte[])args[0];
            byte[] username = (byte[])args[1];
            byte[] password = (byte[])args[2];
            //byte[] encryptionType = (byte[])args[3];

            if (!checkExistingDomain(ownerAddress, domain))
            {
                byte[] domainAddress = ownerAddress.Concat("domain".AsByteArray());
                byte[] usernameAddress = domainAddress.Concat(domain.Concat("username".AsByteArray()));
                byte[] passwordAddress = domainAddress.Concat(domain.Concat("password".AsByteArray()));
                byte[] encryptionTypeAddress = domainAddress.Concat(domain.Concat("encryption".AsByteArray()));

                Runtime.Notify("SetStorage() pubKey", pubKey);
                Runtime.Notify("SetStorage() args[0] - domain", args[0]);
                Runtime.Notify("SetStorage() args[1] - username", args[1]);
                Runtime.Notify("SetStorage() args[2] - password", args[2]);
                //Runtime.Notify("SetStorage() args[3] - encyptionType", args[3]);

                Storage.Put(Storage.CurrentContext, domainAddress, domain);
                Storage.Put(Storage.CurrentContext, usernameAddress, username);
                Storage.Put(Storage.CurrentContext, passwordAddress, password);
                //Storage.Put(Storage.CurrentContext, encryptionTypeAddress, encryptionType);

                Runtime.Notify(domainAddress);
                Runtime.Notify(usernameAddress);
                Runtime.Notify(passwordAddress);

                updateAddressList(ownerAddress, domain);
            }
            return true;
        }
        private static Boolean getStorage(string pubKey, object[] args)
        {
            byte[] ownerAddress = pubKey.AsByteArray();
            byte[] domain = (byte[])args[0];
            Runtime.Notify(ownerAddress);
            Runtime.Notify(domain);
            byte[] storageLocation = ownerAddress.Concat("domain".AsByteArray().Concat(domain));
            Runtime.Notify(storageLocation);
            var domainAddress = Storage.Get(Storage.CurrentContext, storageLocation);
            Runtime.Notify(domainAddress);

            byte[] usernameAddress = storageLocation.Concat("username".AsByteArray());
            byte[] passwordAddress = storageLocation.Concat("password".AsByteArray());

            var result1 = Storage.Get(Storage.CurrentContext, usernameAddress);
            Runtime.Notify(result1);

            Runtime.Notify(Storage.Get(Storage.CurrentContext, usernameAddress));
            Runtime.Notify(Storage.Get(Storage.CurrentContext, passwordAddress));

            //var storage = Storage.Get(Storage.CurrentContext, )
            //Runtime.Notify(storage);

            return true;
        }
        private static Boolean updateAddressList(byte[] domainAddress, byte[] domain)
        {
            var storageValue = Storage.Get(Storage.CurrentContext, domainAddress);
            Runtime.Notify("updateAddressList() storageValueBefore", storageValue);
            storageValue = storageValue.Concat(domain.Concat(",".AsByteArray()));
            Runtime.Notify("updateAddressList() storageValueAfter", storageValue);
            Storage.Put(Storage.CurrentContext, domainAddress, storageValue);

            return true;
        }
        //Checks if the new Domains already exists under the PublicKey-Address
        private static Boolean checkExistingDomain(byte[] domainAddress, byte[] domain)
        {
            var storageValue = Storage.Get(Storage.CurrentContext, domainAddress);
            return Contains(storageValue, domain);
        }

        private static byte[] GetStorage(string pubKey)
        {
            return Storage.Get(Storage.CurrentContext, pubKey);
        }
        
        private static bool Contains(byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return false;

            for (int i = 0; i < self.Length; i++)
            {
                if (IsMatch(self, i, candidate))
                    return true;
            }

            return false;
        }
        
        private static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }
        
        private static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                    || candidate == null
                    || array.Length == 0
                    || candidate.Length == 0
                    || candidate.Length > array.Length;
        }
        
        private static bool Register(string domain, byte[] owner)
        {
            if (!Runtime.CheckWitness(owner)) return false;
            byte[] value = Storage.Get(Storage.CurrentContext, domain);
            if (value != null) return false;
            Storage.Put(Storage.CurrentContext, domain, owner);
            return true;
        }
    }
}
