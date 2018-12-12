using SNTON.Components.CleanUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.Message;
using log4net;
using System.Reflection;
using VI.MFC.Logging;
using SNTON.Constants;
using System.Linq.Expressions;
using SNTON.WebServices.UserInterfaceBackend.Requests;

namespace SNTON.Components.MessageInfo
{
    public class MessageInfo : CleanUpBrokerBase, IMessageInfo
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "MessageEntity";
        private const string DatabaseDbTable = "SNTON.Message";

        Dictionary<int, MessageEntity> _LastMessageDic = new Dictionary<int, MessageEntity>();
        protected override string EntityTableName
        {
            get
            {
                return EntityDbTable;
            }
        }
        public override void ReadBrokerData()
        {
        }
        public override long DeleteRecordsOlderThan(DateTime theDate, long maxRecords)
        {
            int result = 0;
            try
            {
                string sql = $"DELETE {DatabaseDbTable} WHERE ISDELETED=1 AND  CREATED<='{theDate.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}";
                result = RunSqlStatement(null, sql);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("faild to delete", ex);
            }
            return result;
        }
        /// <summary>
        /// Mark the data with the deleted tag
        /// </summary>
        /// <param name="olderThan"></param>
        /// <param name="threadDeleteMaxRecords"></param>
        /// <param name="theSession"></param>
        protected override void MarkDataForDeletion(DateTime olderThan, int threadDeleteMaxRecords, IStatelessSession theSession)
        {

            if (theSession == null)
            {
                BrokerDelegate(() => MarkDataForDeletion(olderThan, threadDeleteMaxRecords, theSession), ref theSession);
                return;
            }
            try
            {
                protData.EnterWriteLock();
                string sql = $"UPDATE {DatabaseDbTable} SET ISDELETED=1 WHERE CREATED<='{olderThan.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}'  AND  ISDELETED=0";
                int result = RunSqlStatement(theSession, sql);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to mark data for the deletion " + DatabaseDbTable, e);
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }
        #region Interface inplementation
        /// <summary>
        /// Get latest message by message level
        /// </summary>
        /// <param name="msgLevel">Message level</param>
        /// <param name="session">Database session</param>
        /// <returns></returns>
        public MessageEntity GetNewMessageByLevel(int msgLevel, IStatelessSession session = null)
        {

            MessageEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetNewMessageByLevel(msgLevel, session), ref session);
                return ret;
            }
            try
            {
                if (_LastMessageDic.ContainsKey(msgLevel))
                {
                    ret = _LastMessageDic[msgLevel];
                    //return _LastMessageDic[msgLevel];
                }

                // try
                // {
                var tmp = ReadList<MessageEntity>(session, string.Format("FROM {0} where MsgLevel = {1} AND IsDeleted={2} order by Id desc", EntityDbTable, msgLevel, Constants.SNTONConstants.DeletedTag.NotDeleted));
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            //}
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get new message", e);
            }
            return ret;
        }
        /// <summary>
        /// Get messages by start index and end index
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="session"></param>
        /// <returns>List of message</returns>
        public List<MessageEntity> GetMessages(long startIndex, long endIndex, IStatelessSession session = null)
        {
            List<MessageEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetMessages(startIndex, endIndex, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<MessageEntity>(session, "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY ID desc) AS RowNumber,* FROM [SNTON].[Message] WHERE  IsDeleted=0)T WHERE T.RowNumber BETWEEN " + startIndex + " AND " + endIndex);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get message", e);
            }
            return ret;
        }
        /// <summary>
        /// Save messages
        /// </summary>
        /// <param name="messages">List of messages</param>
        /// <param name="session">Database session</param>
        /// <returns>void</returns>
        public void SaveMessages(List<MessageEntity> messages, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => SaveMessages(messages, session), ref session);
                return;
            }
            try
            {
                if (messages.Any())
                {
                    var group = messages.GroupBy(x => x.MsgLevel);
                    foreach (var item in group)
                    {
                        var re = item.OrderBy(x => x.Created);
                        var tmp = re.LastOrDefault();
                        if (tmp != null)
                        {
                            if (_LastMessageDic.ContainsKey(tmp.MsgLevel))
                            {
                                _LastMessageDic[tmp.MsgLevel] = tmp;
                            }
                            else
                            {
                                _LastMessageDic.Add(tmp.MsgLevel, tmp);
                            }
                        }
                    }
                    Insert(session, messages);
                }
                logger.InfoMethod(string.Format("Save {0} records messages lists to DB successfully", messages.Count));
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save message", e);
                throw e;
            }

        }

        public List<MessageEntity> GetMessagesBySearch(MessageSearchRequest searchRequest, IStatelessSession session = null)
        {
            List<MessageEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetMessagesBySearch(searchRequest, session), ref session);
                return ret;
            }
            try
            {
                MessageEntity m = new MessageEntity();
                StringBuilder sb = new StringBuilder();
                if (searchRequest.endTime.HasValue)
                    sb.Append(" AND Created" + "<='" + searchRequest.endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                if (searchRequest.startTime.HasValue)
                    sb.Append(" AND Created" + ">='" + searchRequest.startTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                if (!string.IsNullOrEmpty(searchRequest.msgSource))
                    sb.Append(" AND Source" + " = '" + searchRequest.msgSource + "'");
                if (searchRequest.msgLevel != -1)
                    sb.Append(" AND MsgLevel" + " = '" + searchRequest.msgLevel + "'");
                //if (searchRequest.msgLevel != 4)
                //    sb.Append(" AND " + nameof(m.MsgLevel) + "=" + searchRequest.msgLevel);
                if (!string.IsNullOrEmpty(searchRequest.Key))
                    sb.Append(" AND MsgContent" + " LIKE '%" + searchRequest.Key + "%'");
                if (searchRequest.MidStoreage != 0)
                    sb.Append(" AND MidStoreage =" + searchRequest.MidStoreage);

                string sql = "SELECT * FROM [SNTON].[SNTON].[Message]  WHERE ID IN(SELECT MAX(ID) FROM[SNTON].[SNTON].[Message]  GROUP BY [Source],[MsgContent]) AND  ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sb?.ToString() + " ORDER BY ID DESC";
                //sql = "SELECT  ROW_NUMBER() OVER (ORDER BY ID desc) AS RowNumber, * FROM [SNTON].[SNTON].[Message]  WHERE ID IN (SELECT MAX(ID) FROM[SNTON].[SNTON].[Message] GROUP BY [Source],[MsgContent]) AND  ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sb?.ToString() + $"AND RowNumber>={searchRequest.pageSize * searchRequest.pageNumber-1} AND RowNumber<={searchRequest.pageSize * searchRequest.pageNumber} ORDER BY ID DESC";
                //var tmp = ReadSqlList<MessageEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sb?.ToString());
                var tmp = ReadSqlList<MessageEntity>(session, sql);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetMessagesBySearch", e);
            }
            return ret;
        }

        //public List<MessageEntity> GetMessagesByExpression(Expression<Func<MessageEntity, object>> expression, IStatelessSession theSession)
        //{ 
        //    throw new NotImplementedException();
        //}
        #endregion Interface implementation
        #region

        #endregion
        protected override int DeleteDataMarkedDeleted(IStatelessSession theSession = null)
        {
            int resut = 0;
            if (theSession == null)
            {
                resut = BrokerDelegate(() => DeleteDataMarkedDeleted(theSession), ref theSession);
                return resut;
            }
            try
            {
                protData.EnterWriteLock();
                string sql = $"DELETE {DatabaseDbTable} WHERE ISDELETED=1";
                resut = RunSqlStatement(theSession, sql);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to delete data for the deletion " + DatabaseDbTable, e);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return resut;
        }

        public int Add(IStatelessSession session, params MessageEntity[] messages)
        {
            if (session == null)
            {
                int ret = BrokerDelegate(() => Add(session, messages), ref session);
                return ret;
            }

            try
            {
                protData.EnterWriteLock();
                Insert(session, messages.ToList());
                //logger.InfoMethod("添加Messages成功");
                return messages.Length;
            }
            catch (Exception ex)
            {
                logger.WarnMethod("添加Messages失败", ex);
                return 0;
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }

        public List<string> GetMessageSource(IStatelessSession session = null)
        {
            List<MessageEntity> list = new List<MessageEntity>();
            if (session == null)
            {
                var ret = BrokerDelegate(() => GetMessageSource(session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                list = ReadSqlList<MessageEntity>(session, "SELECT * FROM  [SNTON].[SNTON].[Message] WHERE ID IN (SELECT MAX(ID)FROM[SNTON].[SNTON].[Message]GROUP BY[Source])");
                if (list != null && list.Count != 0)
                {
                    var l = from i in list
                            orderby i.Source
                            select i.Source.Trim();
                    return l.ToList();
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return new List<string>();
            }
            finally
            {
                protData.ExitReadLock();
            }
        }

        public Tuple<List<MessageEntity>, int, int> GetMessagesBySearchForPage(MessageSearchRequest searchRequest, IStatelessSession session = null)
        {
            Tuple<List<MessageEntity>, int, int> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetMessagesBySearchForPage(searchRequest, session), ref session);
                return ret;
            }
            try
            {
                MessageEntity m = new MessageEntity();
                StringBuilder sb = new StringBuilder();
                if (searchRequest.endTime.HasValue)
                    sb.Append(" AND Created" + "<='" + searchRequest.endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                if (searchRequest.startTime.HasValue)
                    sb.Append(" AND Created" + ">='" + searchRequest.startTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                if (!string.IsNullOrEmpty(searchRequest.msgSource))
                    sb.Append(" AND Source" + " = '" + searchRequest.msgSource + "'");
                if (searchRequest.msgLevel != -1)
                    sb.Append(" AND MsgLevel" + " = '" + searchRequest.msgLevel + "'");
                //if (searchRequest.msgLevel != 4)
                //    sb.Append(" AND " + nameof(m.MsgLevel) + "=" + searchRequest.msgLevel);
                if (!string.IsNullOrEmpty(searchRequest.Key))
                    sb.Append(" AND Message" + " LIKE '%" + searchRequest.Key + "%'");

                string sql = "SELECT * FROM [SNTON].[SNTON].[Message]  WHERE ID IN(SELECT MAX(ID) FROM[SNTON].[SNTON].[Message]  GROUP BY [Source],[MsgContent]) AND  ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sb?.ToString() + " ORDER BY ID DESC";
                sql = "SELECT  ROW_NUMBER() OVER (ORDER BY ID desc) AS RowNumber, * FROM [SNTON].[SNTON].[Message]  WHERE ID IN (SELECT MAX(ID) FROM[SNTON].[SNTON].[Message] GROUP BY [Source],[MsgContent]) AND  ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sb?.ToString() + $"AND RowNumber>={searchRequest.pageSize * searchRequest.pageNumber - 1} AND RowNumber<={searchRequest.pageSize * searchRequest.pageNumber} ORDER BY ID DESC";
                //var tmp = ReadSqlList<MessageEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sb?.ToString());
                string sqlcount = "SELECT COUNT(1) FROM [SNTON].[SNTON].[Message]  WHERE ID IN (SELECT MAX(ID) FROM[SNTON].[SNTON].[Message] GROUP BY [Source],[MsgContent]) AND  ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sb?.ToString() + " ORDER BY ID DESC";
                var tmp = ReadSqlList<MessageEntity>(session, sql);
                //int count = ReadScalar<int>(session, sqlcount);
                if (tmp.Any())
                {
                    //ret.Item1 = tmp.ToList();
                }
                return new Tuple<List<MessageEntity>, int, int>(tmp, 1, 1);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetMessagesBySearch", e);
            }
            return ret;
        }
    }
}
