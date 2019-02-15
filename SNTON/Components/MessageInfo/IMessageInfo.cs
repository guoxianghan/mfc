using NHibernate;
using SNTON.Entities.DBTables.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using SNTON.WebServices.UserInterfaceBackend.Requests;

namespace SNTON.Components.MessageInfo
{
    public interface IMessageInfo
    {
        /// <summary>
        /// Get latest message by message level
        /// </summary>
        /// <param name="msgLevel">Message level</param>
        /// <param name="session">Database session</param>
        /// <returns></returns>
        MessageEntity GetNewMessageByLevel(int msgLevel, IStatelessSession session = null);

        /// <summary>
        /// Get messages by expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        // List<MessageEntity> GetMessagesByExpression(Expression<Func<MessageEntity, object>> expression, IStatelessSession theSession);
        List<MessageEntity> GetMessagesBySearch(MessageSearchRequest searchRequest, IStatelessSession session = null);
        /// <summary>
        /// 返回当前页,总条数,总页数
        /// </summary>
        /// <param name="searchRequest"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        Tuple<List<MessageEntity>, int, int> GetMessagesBySearchForPage(MessageSearchRequest searchRequest, IStatelessSession session = null);
        /// <summary>
        /// Get messages by start index and end index
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="session"></param>
        /// <returns>List of message</returns>
        List<MessageEntity> GetMessages(long startIndex, long endIndex, IStatelessSession session = null);
        List<string> GetMessageSource(IStatelessSession session = null);
        /// <summary>
        /// Save messages
        /// </summary>
        /// <param name="messages">List of messages</param>
        /// <param name="session">Database session</param>
        /// <returns>void</returns>
        void SaveMessages(List<MessageEntity> messages, IStatelessSession session = null);
        int Add(IStatelessSession session, params MessageEntity[] messages);
    }
}
