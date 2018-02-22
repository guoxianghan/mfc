// Copyright (c) 2009 - 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: EntityBase.cs,v 1.3 2011/09/02 12:58:18 deld Exp $

using System;
using System.Runtime.Serialization;
using SNTON.Constants;

namespace SNTON.Entities.DBTables
{
    /// <summary>
    /// The mother of all our entities. There must not be any entity without
    /// the information shown herein.
    /// </summary>
    [DataContract(Name = "EntityBase", Namespace = "http://Ni-Tech.CN.MFC.WebServices.ServiceAddressProvider")]
    public class EntityBase : ICloneable
    {
        /// <summary>
        /// Unambiguous Id of this entity as primary key
        /// </summary>
        [DataMember]
        public virtual long Id
        {
            get;
            set;
        }

        /// <summary>
        /// When has this entity been created?
        /// </summary>
        [DataMember]
        public virtual DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// When has this entity been updated?
        /// </summary>
        [DataMember]
        public virtual DateTime? Updated
        {
            get;
            set;
        }

        /// <summary>
        /// When has this entity been updated?
        /// </summary>
        [DataMember]
        public virtual DateTime? Deleted
        {
            get;
            set;
        }
        /// <summary>
        /// Version identifier in case non .NET CLR apps are using this entity
        /// </summary>
        //[DataMember]
        //public virtual long Version
        //{
        //get;
        //set;
        //}

        /// <summary>
        /// delete tag , 0 = Not deleted, 1 = Should be deleted
        /// </summary>
        [DataMember]
        public virtual short IsDeleted
        {
            get;
            set;
        } = SNTONConstants.DeletedTag.NotDeleted;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityBase()
        {
        }

        /// <summary>
        /// ICloneable required implementation.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return (EntityBase)MemberwiseClone();
        }
    }
}