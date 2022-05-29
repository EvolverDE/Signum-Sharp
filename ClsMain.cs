using Signum_Sharp.Classes.Crypto;
using Signum_Sharp.Modules;
using Signum_Sharp.Classes;
using Signum_Sharp.Classes.Converters;
using System.Net;
using System.Security.Cryptography;

namespace Signum_Sharp
{
    public class ClsMain
    {
        private static string C_AddressPreFix = "S";
        

        public string C_Node { get; set; } = "";

        private static string? C_PassPhrase = "";
        private static string? C_PrivateKey = "";
        private static string? C_AgreementKey = "";

        public string C_PublicKey { get; set; }
        public ulong C_AccountID { get; set; }
        public string C_Address { get; set; }

        public List<List<string>> C_UTXList { get; set; } = new List<List<string>>();

        public ClsMain(string Node, string PassPhrase)
        {
            C_Node = Node;
            C_PassPhrase = PassPhrase;
            
            List<string> MasterKeys = ModCrypto.GetMasterKeys(PassPhrase);
            C_PrivateKey = MasterKeys[1];
            C_AgreementKey = MasterKeys[2];
            C_PublicKey = MasterKeys[0];
            C_AccountID = ModGlobalFunctions.GetAccountID(MasterKeys[0]);
            C_Address = ModGlobalFunctions.GetAccountRS(MasterKeys[0]);
        }


        #region Blockchain Communication

        #region Basic API

        public string BroadcastTransaction(string TXBytesHexStr)
        {
            string Response = SignumRequest("requestType=broadcastTransaction&transactionBytes=" + TXBytesHexStr);

           

            if (Response.Contains("error"))
                return "error in BroadcastTransaction(): ->\n" + Response;

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
                // TX not OK
                return "error in BroadcastTransaction(): " + Response;


            object UTX = JSON.RecursiveListSearch((List<object>)RespList, "transaction");

            string Returner = "error in BroadcastTransaction(): -> UTX failure";
            if (UTX.GetType().Name == typeof(string).Name)
                Returner = Convert.ToString(UTX);
            else if (UTX.GetType().Name == typeof(bool).Name)
            {
            }
            else if (UTX.GetType().Name == typeof(List<>).Name)
            {
            }

            return Returner;
        }

        public string SignumRequest(string postData)
        {
            try
            {
                //TODO: test the untested function
                HttpClient request = new HttpClient();
                StringContent requestStringContent = new StringContent (postData, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("POST"), C_Node);
                requestMessage.Content = requestStringContent;
                HttpResponseMessage responseMessage = request.Send(requestMessage);
                HttpContent responseContent = responseMessage.Content;

                Stream dataStream = responseContent.ReadAsStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();

                return responseFromServer;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        #endregion

        // ####################################################################################################

        #region Get

        public string GetAccountPublicKeyFromAccountID_RS(string AccountID_RS)
        {
            string Response = SignumRequest("requestType=getAccountPublicKey&account=" + AccountID_RS.Trim());

            if (Response.Contains("error"))
                return "error in GetAccountPublicKeyFromAccountID_RS(): ->\n" + Response;

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
                // TX not OK
                return "error in GetAccountPublicKeyFromAccountID_RS(): " + Response;


            object PublicKey = JSON.RecursiveListSearch((List<object>)RespList, "publicKey").ToString;

            string Returner = "";
            if (PublicKey.GetType().Name == typeof(string).Name)
                Returner = System.Convert.ToString(PublicKey);

            return Returner;
        }

        /// <summary>
        /// Gets the Balance from the given Address (HTML-Tags= coin, account, address, balance, available, pending)
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public List<string> GetBalance(string Address)
        {
            ulong AccountID = 0;

            List<string> ConvAddress = ModGlobalFunctions.ConvertAddress(Address);
            if (ConvAddress.Count > 0)
                AccountID = Convert.ToUInt64(ConvAddress[0]);

            List<string> CoinBal = new List<string>
            {
                "<coin>SIGNA</coin>",
                "<account>" + AccountID.ToString() + "</account>",
                "<address>" + Address + "</address>",
                "<balance>0</balance>",
                "<available>0</available>",
                "<pending>0</pending>"
            };

            if (AccountID == 0)
            {
                return CoinBal;
            }
            else
                return GetBalance(AccountID);
        }

        /// <summary>
        /// Gets the Balance from the given AccountID (HTML-Tags= coin, account, address, balance, available, pending)
        /// </summary>
        /// <param name="AccountID"></param>
        /// <returns></returns>
        public List<string> GetBalance(ulong AccountID)
        {
            
            string Address = ClsReedSolomon.Encode(AccountID);

            List<string> CoinBal = new List<string>
            {
                "<coin>SIGNA</coin>",
                "<account>" + AccountID.ToString() + "</account>",
                "<address>" + ClsMain.C_AddressPreFix + Address + "</address>",
                "<balance>0</balance>",
                "<available>0</available>",
                "<pending>0</pending>"
            };

            string Response = SignumRequest("requestType=getAccount&account=" + AccountID.ToString());

            if (Response.Contains("error"))
            {
                return CoinBal;
            }

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);


            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
            {
                // TX not OK
                return CoinBal;
            }


            string BalancePlanckStr = JSON.RecursiveListSearch((List<object>)RespList, "balanceNQT").ToString();
            double Balance = 0.0;

            try
            {
                Balance = double.Parse(BalancePlanckStr.Insert(BalancePlanckStr.Length - 8, ","));
            }
            catch (Exception ex)
            {
            }

            string AvailablePlanckStr = JSON.RecursiveListSearch((List<object>)RespList, "unconfirmedBalanceNQT").ToString();
            double Available = 0.0;

            try
            {
                Available = double.Parse(AvailablePlanckStr.Insert(AvailablePlanckStr.Length - 8, ","));
            }
            catch (Exception ex)
            {
            }

            double Pending = Available - Balance;

            // (Coin, Account, Address, Balance, Available, Pending)
            CoinBal[0] = "<coin>SIGNA</coin>";
            CoinBal[1] = "<account>" + AccountID.ToString() + "</account>";
            CoinBal[2] = "<address>" + Address.Trim() + "</address>";
            CoinBal[3] = "<balance>" + Balance.ToString() + "</balance>";
            CoinBal[4] = "<available>" + Available.ToString() + "</available>";
            CoinBal[5] = "<pending>" + Pending.ToString() + "</pending>";

            return CoinBal;
        }

        public double GetTXFee(string Message = "")
        {
            double TXFee = 0.00735 * (Math.Floor(Message.Length / (double)176) + 1); // 69

            if (TXFee < 0.01)
                TXFee = 0.01;

            return TXFee;
        }

        public List<List<string>> GetUnconfirmedTransactions()
        {
            string Response = SignumRequest("requestType=getUnconfirmedTransactions");

            if (Response.Contains("error"))
            {
                return new List<List<string>>();
            }

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
            {
                // TX not OK
                return new List<List<string>>();
            }


            object UTX = JSON.RecursiveListSearch((List<object>)RespList, "unconfirmedTransactions");

            List<object> EntryList = new List<object>();

            if (UTX.GetType().Name == typeof(string).Name)
                return new List<List<string>>();
            else if (UTX.GetType().Name == typeof(bool).Name)
                return new List<List<string>>();
            else if (UTX.GetType().Name == typeof(List<object>).Name)
            {
                List<object> TempOBJList = new List<object>();

                foreach (var T_Entry in (List<object>)UTX)
                {
                    List<object> Entry = new List<object>();

                    if (T_Entry.GetType().Name == typeof(List<object>).Name)
                        Entry = (List<object>)T_Entry;

                    if (Entry.Count > 0)
                    {
                        if (Entry[0].ToString() == "type")
                        {
                            if (TempOBJList.Count > 0)
                                EntryList.Add(TempOBJList);

                            TempOBJList = new List<object>()
                    {
                        Entry
                    };
                        }
                        else
                            TempOBJList.Add(Entry);
                    }
                }

                EntryList.Add(TempOBJList);
            }
            else
                return new List<List<string>>();


            List<List<string>> ReturnList = new List<List<string>>();

            foreach (var T_Entry in EntryList)
            {
                List<object> Entry = new List<object>();

                if (T_Entry.GetType().Name == typeof(List<object>).Name)
                    Entry = (List<object>)T_Entry;

                List<string> TempList = new List<string>();

                foreach (var T_SubEntry in Entry)
                {
                    List<object> SubEntry = new List<object>();
                    if (T_SubEntry.GetType().Name == typeof(List<object>).Name)
                        SubEntry = (List<object>)T_SubEntry;

                    if (SubEntry.Count > 0)
                    {
                        switch (SubEntry[0].ToString())
                        {
                            case "type":
                                {
                                    break;
                                }

                            case "subtype":
                                {
                                    break;
                                }

                            case "timestamp":
                                {
                                    TempList.Add("<timestamp>" + SubEntry[1].ToString() + "</timestamp>");
                                    break;
                                }

                            case "deadline":
                                {
                                    break;
                                }

                            case "senderPublicKey":
                                {
                                    break;
                                }

                            case "amountNQT":
                                {
                                    TempList.Add("<amountNQT>" + SubEntry[1].ToString() + "</amountNQT>");
                                    break;
                                }

                            case "feeNQT":
                                {
                                    TempList.Add("<feeNQT>" + SubEntry[1].ToString() + "</feeNQT>");
                                    break;
                                }

                            case "signature":
                                {
                                    break;
                                }

                            case "signatureHash":
                                {
                                    break;
                                }

                            case "fullHash":
                                {
                                    break;
                                }

                            case "transaction":
                                {
                                    TempList.Add("<transaction>" + SubEntry[1].ToString() + "</transaction>");
                                    break;
                                }

                            case "attachment":
                                {
                                    string TMsg = "<attachment>";


                                    List<object> SubSubEntry = new List<object>();

                                    if (SubEntry[1].GetType().Name == typeof(List<object>).Name)
                                        SubSubEntry = (List<object>)SubEntry[1];

                                    if (SubSubEntry.Count > 0)
                                    {
                                        string Message = JSON.RecursiveListSearch(SubSubEntry, "message").ToString();

                                        if (Message.Trim() != "False")
                                        {
                                            string IsText = JSON.RecursiveListSearch(SubSubEntry, "messageIsText").ToString();
                                            TMsg += "<message>" + Message + "</message><isText>" + IsText + "</isText>";
                                        }

                                        object EncMessage = JSON.RecursiveListSearch(SubSubEntry, "encryptedMessage");

                                        // If EncMessage.GetType.Name = GetType(Boolean).Name Then

                                        // Else
                                        if (EncMessage.GetType().Name == typeof(List<object>).Name)
                                        {
                                            List<object> EncryptedMessageList = new List<object>();
                                            if (EncMessage.GetType().Name == typeof(List<object>).Name)
                                                EncryptedMessageList = (List<object>)EncMessage;

                                            string Data = Convert.ToString(JSON.RecursiveListSearch(EncryptedMessageList, "data"));
                                            string Nonce = Convert.ToString(JSON.RecursiveListSearch(EncryptedMessageList, "nonce"));
                                            string IsText = JSON.RecursiveListSearch(SubSubEntry, "isText").ToString();

                                            if (!(Data.Trim() == "False") & !(Nonce.Trim() == "False"))
                                                TMsg += "<data>" + Data + "</data><nonce>" + Nonce + "</nonce><isText>" + IsText + "</isText>";
                                        }
                                        else
                                        {
                                        }
                                    }

                                    TMsg += "</attachment>";

                                    TempList.Add(TMsg);
                                    break;
                                }

                            case "sender":
                                {
                                    TempList.Add("<sender>" + SubEntry[1].ToString() + "</sender>");
                                    break;
                                }

                            case "senderRS":
                                {
                                    TempList.Add("<senderRS>" + SubEntry[1].ToString() + "</senderRS>");
                                    break;
                                }

                            case "recipient":
                                {
                                    TempList.Add("<recipient>" + SubEntry[1].ToString() + "</recipient>");
                                    break;
                                }

                            case "recipientRS":
                                {
                                    TempList.Add("<recipientRS>" + SubEntry[1].ToString() + "</recipientRS>");
                                    break;
                                }

                            case "height":
                                {
                                    TempList.Add("<height>" + SubEntry[1].ToString() + "</height>");
                                    break;
                                }

                            case "version":
                                {
                                    break;
                                }

                            case "ecBlockId":
                                {
                                    break;
                                }

                            case "ecBlockHeight":
                                {
                                    break;
                                }

                            case "block":
                                {
                                    TempList.Add("<block>" + SubEntry[1].ToString() + "</block>");
                                    break;
                                }

                            case "confirmations":
                                {
                                    TempList.Add("<confirmations>" + SubEntry[1].ToString() + "</confirmations>");
                                    break;
                                }

                            case "blockTimestamp":
                                {
                                    break;
                                }

                            case "requestProcessingTime":
                                {
                                    break;
                                }
                        }
                    }
                }

                ReturnList.Add(TempList);
            }

            C_UTXList.Clear();
            C_UTXList.AddRange(ReturnList.ToArray());

            return ReturnList;
        }

        public int GetCurrentBlock()
        {
            
            int BlockHeightInt = 0;

            string Response = SignumRequest("requestType=getMiningInfo");

            if (Response.Contains("error"))
            {
                return 0;
            }

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
            {
                // TX not OK
                return 0;
            }

            object BlockHeightStr = JSON.RecursiveListSearch((List<object>)RespList, "height");

            try
            {
                BlockHeightInt = Convert.ToInt32(BlockHeightStr);
            }
            catch (Exception ex)
            {
                return 0;
            }

            return BlockHeightInt;
        }

        public List<string> GetTransaction(ulong TXID)
        {
            string Response = SignumRequest("requestType=getTransaction&transaction=" + TXID.ToString());

            if (Response.Contains("error"))
            {
                return new List<string>();
            }

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
            {
                // TX not OK
                return new List<string>();
            }



            List<string> TXDetailList = new List<string>();

            foreach (var T_Entry in (List<object>)RespList)
            {
                List<object> Entry = new List<object>();

                if (T_Entry.GetType().Name == typeof(List<object>).Name)
                    Entry = (List<object>)T_Entry;

                if (Entry.Count > 0)
                {
                    switch (Entry[0].ToString())
                    {
                        case "type":
                            {
                                break;
                            }

                        case "subtype":
                            {
                                break;
                            }

                        case "timestamp":
                            {
                                TXDetailList.Add("<timestamp>" + Entry[1].ToString() + "</timestamp>");
                                break;
                            }

                        case "deadline":
                            {
                                break;
                            }

                        case "senderPublicKey":
                            {
                                break;
                            }

                        case "recipient":
                            {
                                TXDetailList.Add("<recipient>" + Entry[1].ToString() + "</recipient>");
                                break;
                            }

                        case "recipientRS":
                            {
                                TXDetailList.Add("<recipientRS>" + Entry[1].ToString() + "</recipientRS>");
                                break;
                            }

                        case "amountNQT":
                            {
                                TXDetailList.Add("<amountNQT>" + Entry[1].ToString() + "</amountNQT>");
                                break;
                            }

                        case "feeNQT":
                            {
                                TXDetailList.Add("<feeNQT>" + Entry[1].ToString() + "</feeNQT>");
                                break;
                            }

                        case "signature":
                            {
                                break;
                            }

                        case "signatureHash":
                            {
                                break;
                            }

                        case "balanceNQT":
                            {
                                TXDetailList.Add("<balanceNQT>" + Entry[1].ToString() + "</balanceNQT>");
                                break;
                            }

                        case "fullHash":
                            {
                                break;
                            }

                        case "transaction":
                            {
                                TXDetailList.Add("<transaction>" + Entry[1].ToString() + "</transaction>");
                                break;
                            }

                        case "attachment":
                            {
                                List<object> Attachments = Entry[1] as List<object>;

                                string AttStr = "<attachment>";

                                if (!(Attachments == null))
                                    AttStr += JSON.JSONListToXMLRecursive(Attachments);

                                AttStr += "</attachment>";

                                TXDetailList.Add(AttStr);
                                break;
                            }

                        case "attachmentBytes":
                            {
                                break;
                            }

                        case "sender":
                            {
                                TXDetailList.Add("<sender>" + Entry[1].ToString() + "</sender>");
                                break;
                            }

                        case "senderRS":
                            {
                                TXDetailList.Add("<senderRS>" + Entry[1].ToString() + "</senderRS>");
                                break;
                            }

                        case "height":
                            {
                                TXDetailList.Add("<height>" + Entry[1].ToString() + "</height>");
                                break;
                            }

                        case "version":
                            {
                                break;
                            }

                        case "ecBlockId":
                            {
                                break;
                            }

                        case "ecBlockHeight":
                            {
                                break;
                            }

                        case "block":
                            {
                                TXDetailList.Add("<block>" + Entry[1].ToString() + "</block>");
                                break;
                            }

                        case "confirmations":
                            {
                                TXDetailList.Add("<confirmations>" + Entry[1].ToString() + "</confirmations>");
                                break;
                            }

                        case "blockTimestamp":
                            {
                                break;
                            }

                        case "requestProcessingTime":
                            {
                                break;
                            }
                    }
                }
            }

            return TXDetailList;
        }

        public List<List<string>> GetAccountTransactions(ulong AccountID, ulong FromTimestamp = 0UL, ulong FirstIndex = 0UL, ulong LastIndex = 0UL)
        {
            string Request = "requestType=getAccountTransactions&account=" + AccountID.ToString();

            if (!(FromTimestamp == 0UL))
                Request += "&timestamp=" + FromTimestamp.ToString();

            if (!(FirstIndex == 0UL))
                Request += "&firstIndex=" + FirstIndex.ToString();

            if (!(LastIndex == 0UL))
                Request += "&lastIndex=" + LastIndex.ToString();

            string Response = SignumRequest(Request);

            if (Response.Contains("error"))
            {
                return new List<List<string>>();
            }

            ClsJSON JSON = new ClsJSON();
            List<List<string>> ReturnList = new List<List<string>>();

            Response = ClsJSON.GetStringBetween(Response, "[", "]", true);

            if (Response.Trim() == "")
                return ReturnList;

            List<string> T_List = ClsJSON.Between2List(Response, "{", "}");
            T_List[1] = T_List[1].Replace("{},", "");
            List<string> JSONStringList = new List<string>();
            JSONStringList.Add(T_List[0]);

            while (T_List[1].Length > 2)
            {
                T_List = ClsJSON.Between2List(T_List[1], "{", "}");

                if (T_List.Count > 0)
                {
                    T_List[1] = T_List[1].Replace("{},", "");
                    JSONStringList.Add(T_List[0]);
                }
            }

            for (int i = 0; i <= JSONStringList.Count - 1; i++)
            {
                string ResponseTX = JSONStringList[i];


                object RespList = JSON.JSONRecursive(ResponseTX);

                object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
                if (Error0.GetType().Name == typeof(bool).Name)
                {
                }
                else if (Error0.GetType().Name == typeof(string).Name)
                {
                    // TX not OK
                    
                    return new List<List<string>>();
                }

                List<string> TempList = new List<string>();

                foreach (var T_SubEntry in (List<object>)RespList)
                {
                    List<object> SubEntry = new List<object>();

                    if (T_SubEntry.GetType().Name == typeof(List<object>).Name)
                        SubEntry = (List<object>)T_SubEntry;

                    if (SubEntry.Count > 0)
                    {
                        switch (true)
                        {
                            case object _ when SubEntry[0].ToString() == "type":
                                {
                                    TempList.Add("<type>" + SubEntry[1].ToString() + "</type>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "timestamp":
                                {
                                    TempList.Add("<timestamp>" + SubEntry[1].ToString() + "</timestamp>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "recipient":
                                {
                                    TempList.Add("<recipient>" + SubEntry[1].ToString() + "</recipient>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "recipientRS":
                                {
                                    TempList.Add("<recipientRS>" + SubEntry[1].ToString() + "</recipientRS>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "amountNQT":
                                {
                                    TempList.Add("<amountNQT>" + SubEntry[1].ToString() + "</amountNQT>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "feeNQT":
                                {
                                    TempList.Add("<feeNQT>" + SubEntry[1].ToString() + "</feeNQT>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "transaction":
                                {
                                    TempList.Add("<transaction>" + SubEntry[1].ToString() + "</transaction>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "attachment":
                                {
                                    string TMsg = "<attachment>";
                                    string Message = JSON.RecursiveListSearch((List<object>)SubEntry[1], "message").ToString();

                                    if (Message.Trim() != "False")
                                    {
                                        string IsText = JSON.RecursiveListSearch((List<object>)SubEntry[1], "messageIsText").ToString();
                                        TMsg += "<message>" + Message + "</message><isText>" + IsText + "</isText>";
                                    }

                                    object EncMessage = JSON.RecursiveListSearch((List<object>)SubEntry[1], "encryptedMessage");

                                    if (EncMessage.GetType().Name == typeof(bool).Name)
                                    {
                                    }
                                    else if (EncMessage.GetType().Name == typeof(List<object>).Name)
                                    {
                                        string Data = Convert.ToString(JSON.RecursiveListSearch((List<object>)EncMessage, "data"));
                                        string Nonce = Convert.ToString(JSON.RecursiveListSearch((List<object>)EncMessage, "nonce"));
                                        string IsText = Convert.ToString(JSON.RecursiveListSearch((List<object>)SubEntry[1], "isText"));

                                        if (!(Data.Trim() == "False") & !(Nonce.Trim() == "False"))
                                            TMsg += "<data>" + Data + "</data><nonce>" + Nonce + "</nonce><isText>" + IsText + "</isText>";
                                    }

                                    TMsg += "</attachment>";
                                    TempList.Add(TMsg);
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "sender":
                                {
                                    TempList.Add("<sender>" + SubEntry[1].ToString() + "</sender>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "senderRS":
                                {
                                    TempList.Add("<senderRS>" + SubEntry[1].ToString() + "</senderRS>");
                                    break;
                                }

                            case object _ when SubEntry[0].ToString() == "confirmations":
                                {
                                    TempList.Add("<confirmations>" + SubEntry[1].ToString() + "</confirmations>");
                                    break;
                                }
                        }
                    }
                }
                ReturnList.Add(TempList);
            }

            return ReturnList;
        }

        public List<string> GetAccountTransactionsRAWList(ulong AccountID, ulong FromTimestamp = 0UL, ulong FirstIndex = 0UL, ulong LastIndex = 0UL)
        {
            string Request = "requestType=getAccountTransactions&account=" + AccountID.ToString();

            if (!(FromTimestamp == 0UL))
                Request += "&timestamp=" + FromTimestamp.ToString();

            if (!(FirstIndex == 0UL))
                Request += "&firstIndex=" + FirstIndex.ToString();

            if (!(LastIndex == 0UL))
                Request += "&lastIndex=" + LastIndex.ToString();

            string Response = SignumRequest(Request);

            if (Response.Contains("error"))
            {
                return new List<string>();
            }

            ClsJSON JSON = new ClsJSON();

            Response = ClsJSON.GetStringBetween(Response, "[", "]", true);

            if (Response.Trim() == "")
                return new List<string>();

            List<string> T_List = ClsJSON.Between2List(Response, "{", "}");
            T_List[1] = T_List[1].Replace("{},", "");
            List<string> JSONStringList = new List<string>();
            JSONStringList.Add(T_List[0]);

            while (T_List[1].Length > 2)
            {
                T_List = ClsJSON.Between2List(T_List[1], "{", "}");

                if (T_List.Count > 0)
                {
                    T_List[1] = T_List[1].Replace("{},", "");
                    JSONStringList.Add(T_List[0]);
                }
            }

            return JSONStringList;
        }

        public List<ulong> GetTransactionIds(ulong Sender, ulong Recipient = 0UL, ulong FromTimestamp = 0UL)
        {
            string Request = "requestType=getTransactionIds&sender=" + Sender.ToString();

            if (!(Recipient == 0UL))
                Request += "&recipient=" + Recipient.ToString();

            if (!(FromTimestamp == 0UL))
                Request += "&timestamp=" + FromTimestamp.ToString();

            string Response = SignumRequest(Request);

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            List<ulong> NuList = new List<ulong>();

            List<object> ResponseList = new List<object>();
            if (RespList.GetType().Name == typeof(List<object>).Name)
                ResponseList = (List<object>)RespList;

            if (ResponseList.Count > 0)
            {
                foreach (var T_Entry in ResponseList)
                {
                    List<object> Entry = new List<object>();

                    if (T_Entry.GetType().Name == typeof(List<object>).Name)
                        Entry = (List<object>)T_Entry;

                    if (Entry.Count > 0)
                    {
                        switch (Entry[0].ToString())
                        {
                            case "transactionIds":
                                {
                                    List<object> TXIDs = new List<object>();

                                    if (Entry[1].GetType().Name == typeof(List<object>).Name)
                                        TXIDs = (List<object>)Entry[1];

                                    if (TXIDs.Count > 0)
                                    {
                                        if (!(TXIDs == null))
                                        {
                                            foreach (var TXID in TXIDs)
                                            {
                                                if (TXID.GetType().Name == typeof(string).Name)
                                                    NuList.Add(Convert.ToUInt64(TXID));
                                                else if (TXIDs.GetType().Name == typeof(List<object>).Name)
                                                {
                                                    List<object> ObjList = TXID as List<object>;

                                                    if (ObjList == null)
                                                    {
                                                        List<string> StrList = TXID as List<string>;

                                                        if (StrList == null)
                                                        {
                                                        }
                                                        else
                                                            foreach (var STXID in StrList)
                                                            {
                                                                if (STXID.GetType().Name == typeof(string).Name)
                                                                    NuList.Add(Convert.ToUInt64(STXID));
                                                                else if (STXID.GetType().Name == typeof(List<object>).Name)
                                                                {
                                                                }
                                                            }
                                                    }
                                                    else
                                                        foreach (var STXID in ObjList)
                                                        {
                                                            if (STXID.GetType().Name == typeof(string).Name)
                                                                NuList.Add(Convert.ToUInt64(STXID));
                                                            else if (STXID.GetType().Name == typeof(List<object>).Name)
                                                            {
                                                            }
                                                        }
                                                }
                                            }
                                        }
                                        else
                                        {
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            return NuList;
        }

        public List<string> GetSmartContractIds()
        {
            string Response = SignumRequest("requestType=getATIds");

            if (Response.Contains("error"))
            {
                
                return new List<string>();
            }

            ClsJSON JSON = new ClsJSON();

            List<object> RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch(RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
            {
                // TX not OK
                
                return new List<string>();
            }


            foreach (var T_Entry in RespList)
            {
                List<object> Entry = new List<object>();

                if (T_Entry.GetType().Name == typeof(List<object>).Name)
                    Entry = (List<object>)T_Entry;

                if (Entry.Count > 0)
                {
                    if (Entry[0].GetType().Name == typeof(string).Name)
                    {
                        if (Entry[0].ToString() == "atIds")
                        {
                            List<object> SubEntry = new List<object>();

                            if (Entry[1].GetType().Name == typeof(List<object>).Name)
                                SubEntry = (List<object>)Entry[1];

                            if (SubEntry.Count > 0)
                            {
                                List<string> RetList = new List<string>();

                                if (SubEntry[0].GetType().Name == typeof(List<string>).Name)
                                    RetList = (List<string>)SubEntry[0];

                                return RetList;
                            }
                        }
                    }
                }
            }

            return new List<string>();
        }

        public List<string> GetSmartContractDetails(ulong SmartContractID)
        {
            string Response = SignumRequest("requestType=getATDetails&at=" + SmartContractID.ToString());

            if (Response.Contains("error"))
            {
                return new List<string>();
            }

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
            {
                // TX not OK
                
                return new List<string>();
            }

            List<string> SmartContractDetailList = new List<string>();

            foreach (var T_Entry in (List<object>)RespList)
            {
                List<object> Entry = new List<object>();

                if (T_Entry.GetType().Name == typeof(List<object>).Name)
                    Entry = (List<object>)T_Entry;

                if (Entry.Count > 0)
                {
                    switch (Entry[0].ToString())
                    {
                        case "creator":
                            {
                                SmartContractDetailList.Add("<creator>" + Entry[1].ToString() + "</creator>");
                                break;
                            }

                        case "creatorRS":
                            {
                                SmartContractDetailList.Add("<creatorRS>" + Entry[1].ToString() + "</creatorRS>");
                                break;
                            }

                        case "at":
                            {
                                SmartContractDetailList.Add("<at>" + Entry[1].ToString() + "</at>");
                                break;
                            }

                        case "atRS":
                            {
                                SmartContractDetailList.Add("<atRS>" + Entry[1].ToString() + "</atRS>");
                                break;
                            }

                        case "atVersion":
                            {
                                break;
                            }

                        case "name":
                            {
                                SmartContractDetailList.Add("<name>" + Entry[1].ToString() + "</name>");
                                break;
                            }

                        case "description":
                            {
                                SmartContractDetailList.Add("<description>" + Entry[1].ToString() + "</description>");
                                break;
                            }

                        case "machineCode":
                            {
                                SmartContractDetailList.Add("<machineCode>" + Entry[1].ToString() + "</machineCode>");

                                //if (!(C_ReferenceMachineCode == null))
                                //{
                                //    if (C_ReferenceMachineCode.Trim == Entry[1].ToString().Trim())
                                //        SmartContractDetailList.Add("<referenceMachineCode>True</referenceMachineCode>");
                                //    else
                                //        SmartContractDetailList.Add("<referenceMachineCode>False</referenceMachineCode>");
                                //}
                                //else
                                    SmartContractDetailList.Add("<referenceMachineCode>False</referenceMachineCode>");
                                break;
                            }

                        case "machineData":
                            {
                                SmartContractDetailList.Add("<machineData>" + Entry[1].ToString() + "</machineData>");
                                break;
                            }

                        case "balanceNQT":
                            {
                                SmartContractDetailList.Add("<balanceNQT>" + Entry[1].ToString() + "</balanceNQT>");
                                break;
                            }

                        case "prevBalanceNQT":
                            {
                                break;
                            }

                        case "nextBlock":
                            {
                                break;
                            }

                        case "frozen":
                            {
                                SmartContractDetailList.Add("<frozen>" + Entry[1].ToString() + "</frozen>");
                                break;
                            }

                        case "running":
                            {
                                SmartContractDetailList.Add("<running>" + Entry[1].ToString() + "</running>");
                                break;
                            }

                        case "stopped":
                            {
                                SmartContractDetailList.Add("<stopped>" + Entry[1].ToString() + "</stopped>");
                                break;
                            }

                        case "finished":
                            {
                                SmartContractDetailList.Add("<finished>" + Entry[1].ToString() + "</finished>");
                                break;
                            }

                        case "dead":
                            {
                                SmartContractDetailList.Add("<dead>" + Entry[1].ToString() + "</dead>");
                                break;
                            }

                        case "minActivation\"":
                            {
                                break;
                            }

                        case "creationBlock\"":
                            {
                                break;
                            }

                        case "requestProcessingTime":
                            {
                                break;
                            }
                    }
                }
            }

            return SmartContractDetailList;
        }

        #endregion

        // ####################################################################################################

        #region Message

        public string SendMoney(string SenderPublicKey, ulong RecipientID, double Amount, double Fee = 0.0, string Message = "", bool MessageIsText = true, string RecipientPublicKey = "")
        {

            string PublicKey = SenderPublicKey; 
            string AmountNQT = Dbl2Planck(Amount).ToString();

            if (Fee == 0.0)
                Fee = GetTXFee(Message);

            string FeeNQT = Dbl2Planck(Fee).ToString();

            string postDataRL = "requestType=sendMoney";
            postDataRL += "&recipient=" + RecipientID.ToString();
            postDataRL += "&amountNQT=" + AmountNQT;
            // postDataRL += "&secretPhrase=" + C_PassPhrase
            postDataRL += "&publicKey=" + PublicKey; // <<< debug errormaker
            postDataRL += "&feeNQT=" + FeeNQT;
            postDataRL += "&deadline=60";
            // postDataRL += "&referencedTransactionFullHash="
            // postDataRL += "&broadcast="

            if (!(Message.Trim() == ""))
            {
                postDataRL += "&message=" + Message;
                postDataRL += "&messageIsText=" + MessageIsText.ToString();
            }

            // postDataRL += "&messageToEncrypt="
            // postDataRL += "&messageToEncryptIsText="
            // postDataRL += "&encryptedMessageData="
            // postDataRL += "&encryptedMessageNonce="
            // postDataRL += "&messageToEncryptToSelf="
            // postDataRL += "&messageToEncryptToSelfIsText="
            // postDataRL += "&encryptToSelfMessageData="
            // postDataRL += "&encryptToSelfMessageNonce="


            if (!(RecipientPublicKey.Trim() == ""))
                postDataRL += " &recipientPublicKey=" + RecipientPublicKey;

            string Response = SignumRequest(postDataRL);

            return Response;
        }

        public string SendMessage(string SenderPublicKey, string SenderAgreementKey, ulong RecipientID, string Message, bool MessageIsText = true, bool Encrypt = false, double Fee = 0.0, string RecipientPublicKey = "")
        {
            if (RecipientPublicKey == "")
                RecipientPublicKey = GetAccountPublicKeyFromAccountID_RS(RecipientID.ToString());

            if (RecipientPublicKey.Trim() == "" | RecipientPublicKey.Trim() == "0000000000000000000000000000000000000000000000000000000000000000" | RecipientPublicKey.Contains("error"))
            {
                Encrypt = false;
                RecipientPublicKey = "";
            }

            
            string PublicKey = SenderPublicKey; 
            string AgreementKey = SenderAgreementKey;

            string postDataRL = "requestType=sendMessage";
            postDataRL += "&recipient=" + RecipientID.ToString();
            // postDataRL += "&secretPhrase=" + C_PassPhrase
            postDataRL += "&publicKey=" + PublicKey;

            postDataRL += "&deadline=60";
            // postDataRL += "&referencedTransactionFullHash="
            // postDataRL += "&broadcast="

            if (Encrypt)
            {
                // postDataRL += "&messageToEncrypt=" + Message
                postDataRL += "&messageToEncryptIsText=" + MessageIsText.ToString();

                string[] EncryptedMessage_Nonce = EncryptMessage(Message, RecipientPublicKey, AgreementKey);

                postDataRL += "&encryptedMessageData=" + EncryptedMessage_Nonce[0];
                postDataRL += "&encryptedMessageNonce=" + EncryptedMessage_Nonce[1];

                if (Fee == 0.0)
                    Fee = GetTXFee(EncryptedMessage_Nonce[0] + EncryptedMessage_Nonce[1]);
            }
            else
            {
                if (Fee == 0.0)
                    Fee = GetTXFee(Message);

                postDataRL += "&message=" + Message;
                postDataRL += "&messageIsText=" + MessageIsText.ToString();
            }

            string FeeNQT = Dbl2Planck(Fee).ToString();
            postDataRL += "&feeNQT=" + FeeNQT;

            // postDataRL += "&messageToEncryptToSelf="
            // postDataRL += "&messageToEncryptToSelfIsText="
            // postDataRL += "&encryptToSelfMessageData="
            // postDataRL += "&encryptToSelfMessageNonce="

            if (!(RecipientPublicKey.Trim() == ""))
                postDataRL += "&recipientPublicKey=" + RecipientPublicKey;


            string Response = SignumRequest(postDataRL);

            return Response;
        }

        public string ReadMessage(ulong TXID, ulong AccountID)
        {
            string postDataRL = "requestType=getTransaction&transaction=" + TXID.ToString();
            string Response = SignumRequest(postDataRL);

            if (Response.Contains("error"))
                return "error in ReadMessage(): ->\n" + Response;

            Response = Response.Replace(@"\", "");

            ClsJSON JSON = new ClsJSON();

            object RespList = JSON.JSONRecursive(Response);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
                // TX not OK
                return "error in ReadMessage(): " + Response;


            object EncryptedMsg = JSON.RecursiveListSearch((List<object>)RespList, "encryptedMessage");

            string SenderID = Convert.ToString(JSON.RecursiveListSearch((List<object>)RespList, "sender"));
            string RecipientID = Convert.ToString(JSON.RecursiveListSearch((List<object>)RespList, "recipient"));

            if (AccountID == Convert.ToUInt64(SenderID))
                AccountID = Convert.ToUInt64(RecipientID);
            else if (AccountID == Convert.ToUInt64(RecipientID))
                AccountID = Convert.ToUInt64(SenderID);

            string AccountPublicKey = GetAccountPublicKeyFromAccountID_RS(AccountID.ToString());

            if (AccountPublicKey.Contains("error"))
                return "error in ReadMessage(): -> no PublicKey for " + AccountID.ToString();

            string ReturnStr = "";

            if (EncryptedMsg.GetType().Name == typeof(string).Name)
                ReturnStr = EncryptedMsg.ToString();
            else if (EncryptedMsg.GetType().Name == typeof(bool).Name)
                ReturnStr = Convert.ToString(JSON.RecursiveListSearch((List<object>)RespList, "message"));
            else
            {
                string Data = Convert.ToString(JSON.RecursiveListSearch((List<object>)EncryptedMsg, "data"));
                string Nonce = Convert.ToString(JSON.RecursiveListSearch((List<object>)EncryptedMsg, "nonce"));

                string DecryptedMsg = DecryptFrom(AccountPublicKey, Data, Nonce);

                if (DecryptedMsg.Contains("error"))
                    return "error in ReadMessage(): ->\n" + DecryptedMsg;
                else if (DecryptedMsg.Contains("warning"))
                    return "warning in ReadMessage(): ->\n" + DecryptedMsg;

                if (!(ModGlobalFunctions.MessageIsHEXString(DecryptedMsg)))
                    ReturnStr = DecryptedMsg;
            }

            return ReturnStr;
        }

        #endregion

        #endregion

        // ####################################################################################################

        #region Convert tools

        public static List<string> ConvertUnsignedTXToList(string UnsignedTX)
        {
            
            ClsJSON JSON = new ClsJSON();
            object RespList = JSON.JSONRecursive(UnsignedTX);

            object Error0 = JSON.RecursiveListSearch((List<object>)RespList, "errorCode");
            if (Error0.GetType().Name == typeof(bool).Name)
            {
            }
            else if (Error0.GetType().Name == typeof(string).Name)
            {
                // TX not OK
                return new List<string>();
            }

            List<string> TXDetailList = new List<string>();

            foreach (var T_Entry in (List<object>)RespList)
            {
                List<object> Entry = new List<object>();

                if (T_Entry.GetType().Name == typeof(List<object>).Name)
                    Entry = (List<object>)T_Entry;


                switch (Entry[0].ToString())
                {
                    case "broadcasted":
                        {
                            break;
                        }

                    case "unsignedTransactionBytes":
                        {
                            TXDetailList.Add("<unsignedTransactionBytes>" + Entry[1].ToString() + "</unsignedTransactionBytes>");
                            break;
                        }

                    case "transactionJSON":
                        {
                            List<object> SubEntry = new List<object>();

                            if (Entry[1].GetType().Name == typeof(List<object>).Name)
                                SubEntry = (List<object>)Entry[1];

                            string Type = Convert.ToString(JSON.RecursiveListSearch(SubEntry, "type"));
                            string SubType = Convert.ToString(JSON.RecursiveListSearch(SubEntry, "subtype"));
                            string Timestamp = Convert.ToString(JSON.RecursiveListSearch(SubEntry, "timestamp"));
                            // Dim Deadline As String = RecursiveSearch(Entry(1), "deadline")
                            // Dim senderPublicKey As String = RecursiveSearch(Entry(1), "senderPublicKey")
                            string AmountNQT = Convert.ToString(JSON.RecursiveListSearch(SubEntry, "amountNQT"));
                            string FeeNQT = Convert.ToString(JSON.RecursiveListSearch(SubEntry, "feeNQT"));
                            // Dim Signature As String = RecursiveSearch(Entry(1), "signature")
                            // Dim SignatureHash As String = RecursiveSearch(Entry(1), "signatureHash")
                            // Dim FullHash As String = RecursiveSearch(Entry(1), "fullHash")
                            // Dim Transaction As String = TX ' RecursiveSearch(Entry(1), "transaction")
                            // Dim Attachments = RecursiveSearch(Entry(1), "attachment")

                            List<object> Attachments = JSON.RecursiveListSearch(SubEntry, "attachment") as List<object>;
                            string AttStr = "<attachment>";
                            if (!(Attachments is null))
                            {
                                foreach (var Attachment in Attachments)
                                {
                                    List<string> AttList = Attachment as List<string>;
                                    if (!(AttList is null))
                                    {
                                        if (AttList.Count > 1)
                                            AttStr += "<" + AttList[0] + ">" + AttList[1] + "</" + AttList[0] + ">";
                                    }
                                }
                            }

                            AttStr += "</attachment>";

                            // Dim SenderID As String = JSON.RecursiveListSearch(Entry(1), "sender")
                            // Dim SenderRS As String = JSON.RecursiveListSearch(Entry(1), "senderRS")
                            // Dim Height As String = JSON.RecursiveListSearch(Entry(1), "height")
                            // Dim Version As String = JSON.RecursiveListSearch(Entry(1), "version")
                            // Dim ECBlockID As String = JSON.RecursiveListSearch(Entry(1), "ecBlockId")
                            // Dim ECBlockHeight As String = JSON.RecursiveListSearch(Entry(1), "ecBlockHeight")


                            TXDetailList.Add("<type>" + Type + "</type>");
                            TXDetailList.Add("<subtype>" + SubType + "</subtype>");
                            TXDetailList.Add("<timestamp>" + Timestamp + "</timestamp>");
                            // TXDetailList.Add("<deadline>" + Deadline + "</deadline>")
                            // TXDetailList.Add("<senderPublicKey>" + senderPublicKey + "</senderPublicKey>")
                            TXDetailList.Add("<amountNQT>" + AmountNQT + "</amountNQT>");
                            TXDetailList.Add("<feeNQT>" + FeeNQT + "</feeNQT>");
                            // TXDetailList.Add("<signature>" + Signature + "</signature>")
                            // TXDetailList.Add("<signatureHash>" + SignatureHash + "</signatureHash>")
                            // TXDetailList.Add("<fullHash>" + FullHash + "</fullHash>")
                            // TXDetailList.Add("<transaction>" + Transaction + "</transaction>")
                            TXDetailList.Add(AttStr);

                            break;
                        }

                    case "requestProcessingTime":
                        {
                            break;
                        }
                }
            }

            return TXDetailList;
        }

        public static string GetUnixTimestamp()
        {
            double UnixTime;
            UnixTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            string UnixTimeString = UnixTime.ToString();
            if (UnixTimeString.Contains(","))
                UnixTimeString = UnixTimeString.Remove(UnixTimeString.IndexOf(","));

            return UnixTimeString;
        }
        public static ulong TimeToUnix(DateTime dteDate)
        {
            if (dteDate.IsDaylightSavingTime() == true)
            {
                dteDate = dteDate.AddHours(-1);
            }

            DateTime Blockchainstart = Convert.ToDateTime("2014-08-11 04:00:16");

            if (Blockchainstart > dteDate)
            {
                TimeSpan ts = Blockchainstart - dteDate;
                return Convert.ToUInt64(ts.TotalSeconds);
            }
            else
            {
                TimeSpan ts = dteDate - Blockchainstart;
                return Convert.ToUInt64(ts.TotalSeconds);
            }

        }
        public static DateTime UnixToTime(string strUnixTime)
        {
            DateTime Blockchainstart = Convert.ToDateTime("11.08.2014 04:00:16");
            DateTime RealTime = Blockchainstart.AddSeconds(Convert.ToDouble(strUnixTime));

            return RealTime;
        }
        public static string ULng2String(ulong Lng)
        {
            byte[] MsgByteAry = BitConverter.GetBytes(Lng);
            List<byte> MsgByteList = new List<byte>(MsgByteAry);

            MsgByteList.Reverse();

            string MsgStr = System.Text.Encoding.UTF8.GetString(MsgByteList.ToArray());

            MsgStr = MsgStr.Replace(Convert.ToChar(0).ToString(), "");

            return MsgStr;
        }
        public static ulong String2ULng(string input, bool Reverse = true)
        {
            List<byte> ByteAry = System.Text.Encoding.UTF8.GetBytes(input).ToList();

            if (Reverse)
                ByteAry.Reverse();

            for (int i = ByteAry.Count; i <= 15; i++)
                ByteAry.Add(0);

            ulong MsgLng = BitConverter.ToUInt64(ByteAry.ToArray(), 0);

            return MsgLng;
        }
        public static string ULng2HEX(ulong ULng)
        {
            string RetStr = "";

            List<byte> ParaBytes = BitConverter.GetBytes(ULng).ToList();

            foreach (byte ParaByte in ParaBytes)
            {
                string T_RetStr = Convert.ToHexString(new byte[] {ParaByte});

                if (T_RetStr.Length < 2)
                    T_RetStr = "0" + T_RetStr;

                RetStr += T_RetStr;
            }

            return RetStr.ToLower();
        }
        public static ulong HEX2ULng(string HEX)
        {
            ulong T_ULong = 0UL;

            List<byte> ByteList = new List<byte>();

            for (int j = 0; j <= Convert.ToInt32(HEX.Length / (double)2) - 1; j++)
            {
                string HEXStr = HEX.Substring(j * 2, 2);

                byte HEXByte = Convert.ToByte(HEXStr, 16);
                ByteList.Add(HEXByte);
            }

            T_ULong = BitConverter.ToUInt64(ByteList.ToArray(), 0);

            return T_ULong;
        }
        public static string ByteAry2HEX(byte[] BytAry)
        {
            string RetStr = "";

            List<byte> ParaBytes = BytAry.ToList();

            foreach (byte ParaByte in ParaBytes)
            {
                string T_RetStr = Convert.ToHexString(new byte[] { ParaByte });

                if (T_RetStr.Length < 2)
                    T_RetStr = "0" + T_RetStr;

                RetStr += T_RetStr;
            }

            return RetStr.ToLower();
        }
        public static string String2HEX(string input)
        {
            ulong inpLng = String2ULng(input, false);

            return ULng2HEX(inpLng);
        }

        public static string HEXStr2String(string input)
        {
            string RetStr = "";
            int Ungerade = input.Length % 2;

            if (Ungerade == 1)
                input += "0";

            for (int j = 0; j <= Convert.ToInt32(input.Length / (double)2) - 1; j++)
            {
                string HEXStr = input.Substring(j * 2, 2);

                byte HEXByte = Convert.ToByte(HEXStr, 16);

                RetStr += Convert.ToString(HEXByte);
            }

            return RetStr.Replace(Convert.ToChar(0).ToString(), "");
        }
        public static List<ulong> DataStr2ULngList(string ParaStr)
        {
            int Ungerade = ParaStr.Length % 16;

            while (!(Ungerade == 0))
            {
                ParaStr += "0";
                Ungerade = ParaStr.Length % 16;
            }


            List<ulong> RetList = new List<ulong>();
            try
            {
                double HowMuchParas = ParaStr.Length / (double)16;

                for (int i = 0; i <= Convert.ToInt32(HowMuchParas) - 1; i++)
                {
                    string Parameter = ParaStr.Substring(i * 16, 16);

                    List<byte> LittleEndianHEXList = new List<byte>();

                    for (int j = 0; j <= 7; j++)
                    {
                        string HEXStr = Parameter.Substring(j * 2, 2);

                        byte HEXByte = Convert.ToByte(HEXStr, 16);

                        LittleEndianHEXList.Add(HEXByte);
                    }

                    ulong BE = BitConverter.ToUInt64(LittleEndianHEXList.ToArray(), 0);

                    RetList.Add(BE);
                }

                return RetList;
            }
            catch
            {
                return RetList;
            }
        }
        public static string ULngList2DataStr(List<ulong> ULngList)
        {
            string RetStr = "";

            foreach (ulong ULn in ULngList)
                RetStr += ULng2HEX(ULn);

            return RetStr.ToLower();
        }
        
        /// <summary>
        /// Hashing Inputkey and converting them into List(Of ULong)(FirstULongKey, SecondULongKey, HashULong)
        /// </summary>
        /// <param name="InputKey"></param>
        /// <returns></returns>
        public static List<ulong> GetSHA256_64(string InputKey)
        {
            List<byte> InputBytes = System.Text.Encoding.ASCII.GetBytes(InputKey).ToList();

            for (int i = InputBytes.Count; i <= 16; i++)
                InputBytes.Add(0);

            ulong FirstULong = BitConverter.ToUInt64(InputBytes.ToArray(), 0);
            ulong SecondULong = BitConverter.ToUInt64(InputBytes.ToArray(), 8);

            byte[] FirstULongBytes = BitConverter.GetBytes(FirstULong);
            byte[] SecondULongBytes = BitConverter.GetBytes(SecondULong);


            List<byte> ByteList = new List<byte>();
            ByteList.AddRange(FirstULongBytes);
            ByteList.AddRange(SecondULongBytes);

            // Dim test As String = System.Text.Encoding.ASCII.GetString(ByteList.ToArray)

            ByteList.AddRange(new List<byte>{(byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0});
            ByteList.AddRange(new List<byte>{(byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0});


            SHA256 Sha256 = SHA256.Create();
            List<byte> Hash = Sha256.ComputeHash(ByteList.ToArray()).ToList();

            ulong HashULong = BitConverter.ToUInt64(Hash.ToArray(), 0);

            return new List<ulong>
            {
                FirstULong,
                SecondULong,
                HashULong
            };
        }
        public static ulong Dbl2Planck(double Signa)
        {
            if (double.IsInfinity(Signa))
                Signa = 0.0;

            ulong Planck = Convert.ToUInt64(Signa * 100000000UL);
            return Planck;
        }
        public static double Planck2Dbl(ulong Planck)
        {
            double Signa = Planck / 100000000UL;
            return Signa;
        }

        #endregion

        // ####################################################################################################

        #region Toolfunctions

        private struct S_Sorter
        {
            public ulong Timestamp;
            public ulong TXID;
        }

        private List<List<string>> SortTimeStamp(List<List<string>> input)
        {
            List<S_Sorter> TSSort = new List<S_Sorter>();

            for (int i = 0; i <= input.Count - 1; i++)
            {
                List<string> Entry = input[i];

                ulong T_Timestamp = ClsJSON.GetULongBetweenFromList(Entry, "<timestamp>", "</timestamp>");
                ulong T_Transaction = ClsJSON.GetULongBetweenFromList(Entry, "<transaction>", "</transaction>");

                S_Sorter NuSort = new S_Sorter();
                NuSort.Timestamp = T_Timestamp;
                NuSort.TXID = T_Transaction;

                TSSort.Add(NuSort);
            }

            TSSort = TSSort.OrderBy(s => s.Timestamp).ToList();

            List<List<string>> SReturnList = new List<List<string>>();

            foreach (var sort in TSSort)
            {
                for (int i = 0; i <= input.Count - 1; i++)
                {
                    var retent = input[i];

                    ulong T_Timestamp = ClsJSON.GetULongBetweenFromList(retent, "<timestamp>", "</timestamp>");
                    ulong T_Transaction = ClsJSON.GetULongBetweenFromList(retent, "<transaction>", "</transaction>");

                    if (T_Timestamp == sort.Timestamp & T_Transaction == sort.TXID)
                    {
                        SReturnList.Add(retent);
                        break;
                    }
                }
            }

            return SReturnList;
        }

        #endregion

        // ####################################################################################################

        #region Encrypt / Decrypt tools

        /// <summary>
        /// Encrypts Message from Parameters 0=Data; 1=Nonce
        /// </summary>
        /// <param name="Plaintext"></param>
        /// <param name="RecipientPublicKeyHex"></param>
        /// <param name="SenderAgreementKeyHex"></param>
        /// <returns></returns>
        public string[] EncryptMessage(string Plaintext, string RecipientPublicKeyHex, string SenderAgreementKeyHex)
        {
            byte[] PlaintextBytes = ModGlobalFunctions.HEXStringToByteArray(ModGlobalFunctions.StringToHEXString(Plaintext));
            string[] EncryptedMessage_Nonce = EncryptData(PlaintextBytes, RecipientPublicKeyHex, SenderAgreementKeyHex);

            return EncryptedMessage_Nonce;
        }
        /// <summary>
        /// Encrypt Data from Parameters 0=Data; 1=Nonce
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="RecipientPublicKeyHex"></param>
        /// <param name="SenderAgreementKeyHex"></param>
        /// <returns></returns>
        public string[] EncryptData(byte[] Data, string RecipientPublicKeyHex, string SenderAgreementKeyHex)
        {
            ClsCurve25519 Curve = new ClsCurve25519();
            byte[] SharedKeyBytes = new byte[32];
            Curve.GetSharedSecret(ref SharedKeyBytes, ModGlobalFunctions.HEXStringToByteArray(SenderAgreementKeyHex), ModGlobalFunctions.HEXStringToByteArray(RecipientPublicKeyHex));
            Data = ClsGZip.Compress(Data);

            byte[] Nonce =ModGlobalFunctions.RandomBytes(31);
            string NonceHexStr = ModGlobalFunctions.ByteArrayToHEXString(Nonce);

            byte[] SharedKey = new byte[32];
            for (int i = 0; i <= 32 - 1; i++)
                SharedKey[i] = (byte)(SharedKeyBytes[i] ^ Nonce[i]);


            ClsAES AESH = new ClsAES();
            byte[] OutputBytes = AESH.AES_Encrypt(Data, SharedKey);
            string OutputHexStr = ModGlobalFunctions.ByteArrayToHEXString(OutputBytes);

            return new[] { OutputHexStr, NonceHexStr };
        }

        public string DecryptFrom(string PublicKey, string data, string nonce)
        {
            if (C_PassPhrase == "")
            {
                if (C_PublicKey  == PublicKey)
                {
                    return "";
                }
                    

                string DecryptedMsg = DecryptMessage(data, nonce, PublicKey, C_AgreementKey );

                if (DecryptedMsg.Contains("error"))
                    return "error in DecryptFrom(): ->\n" + DecryptedMsg;
                else if (!ModGlobalFunctions.MessageIsHEXString(DecryptedMsg))
                    return DecryptedMsg;
                else
                    return "error in DecryptFrom(): ->\n" + DecryptedMsg;
            }
            else
                return "warning in DecryptFrom(): -> no Keys";
        }
        public string DecryptMessage(string EncryptedMessage, string Nonce, string SenderPublicKeyHex, string RecipientAgreementKeyHex)
        {
            try
            {
                byte[] EncryptedMessageBytes = ModGlobalFunctions.HEXStringToByteArray(EncryptedMessage);
                byte[] NonceBytes = ModGlobalFunctions.HEXStringToByteArray(Nonce);

                string PlainText = DecryptData(EncryptedMessageBytes, NonceBytes, SenderPublicKeyHex, RecipientAgreementKeyHex);

                return PlainText;
            }
            catch (Exception ex)
            {
                return "error in SignumNET.DecryptMessage(): " + ex.Message;
            }
        }
        public string DecryptData(byte[] Data, byte[] Nonce, string SenderPublicKeyHex, string RecipientAgreementKeyHex)
        {
            ClsCurve25519 Curve = new ClsCurve25519();
            byte[] SharedKeyBytes = new byte[32];
            Curve.GetSharedSecret(ref SharedKeyBytes, ModGlobalFunctions.HEXStringToByteArray(RecipientAgreementKeyHex), ModGlobalFunctions.HEXStringToByteArray(SenderPublicKeyHex));

            byte[] CompressedPlaintext = Decrypt(Data, Nonce, SharedKeyBytes);

            string PlainText = ClsGZip.Inflate(CompressedPlaintext);

            return PlainText;
        }
        public byte[] Decrypt(byte[] ivCiphertext, byte[] nonce, byte[] sharedKeyOrig)
        {

            byte[] SharedKey = sharedKeyOrig.ToArray();

            for (int i = 0; i <= 32 - 1; i++)
                SharedKey[i] = (byte)(SharedKey[i] ^ nonce[i]);

            SHA256 Sha256 = SHA256.Create();
            byte[] Key = Sha256.ComputeHash(SharedKey);
            byte[] IV = new byte[16];
            byte[] Buffer = new byte[ivCiphertext.Length - 1 - 16 + 1];

            Array.Copy(ivCiphertext, IV, 16);
            Array.Copy(ivCiphertext, 16, Buffer, 0, ivCiphertext.Length - 16);


            ClsAES AESH = new ClsAES();
            var DecryptBytes = AESH.AES_Decrypt(Buffer, SharedKey, IV);

            return DecryptBytes;
        }

        #endregion

    }
}