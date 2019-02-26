
// Copyright (c) 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of 
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Collections.Generic;
using SNTON.WebServices;
using SNTON.WebServices.UserInterfaceBackend.Models; 

namespace SNTON.WebServices.UserInterfaceBackend.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageResponse : ResponseBase
    {
        /// <summary>
        /// 消息总共的页数
        /// </summary>
        public int pageCount { get; set; }
        /// <summary>
        /// 总条数
        /// </summary>
        public int CountNumber { get; set; }
        /// <summary>
        /// 消息清单
        /// </summary>
        public IList<MessageDataUI> data;

        public MessageResponse()
        {
            pageCount = 0;
            data = new List<MessageDataUI>();
        }
    }
}
