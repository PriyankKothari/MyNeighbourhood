﻿// Copyright (c) 2010. Rusanu Consulting LLC  
// http://code.google.com/p/linqtocache/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace LinqToCache
{
    /// <summary>
    /// A cached query entry in the caching dictionary
    /// This keeps track of the query result, the time added and current validity state
    /// </summary>
    /// <typeparam name="T">Type of the query result enumeration</typeparam>
    internal class CachedEntry<T>
    {
        /// <summary>
        /// Uniquely identifier of this CachedEntry
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// The query result
        /// </summary>
        public IEnumerable<T> List { get; set; }

        /// <summary>
        /// Current validity state.
        /// When this is ECachedEntryState.Invalid, the entry should be ignored if found in cache.
        /// </summary>
        public volatile ECachedEntryState State = ECachedEntryState.None;

        /// <summary>
        /// UTC time the entry was added to cache
        /// </summary>
        public DateTime UtcTimeAdded { get; set; }

        /// <summary>
        /// Event raised when the query is invalidated by the server.
        /// </summary>
        public EventHandler<CachedQueryEventArgs> OnInvalidated { get; set; }

        /// <summary>
        /// Arbitrary object to be passed as an argument to the OnInvalidated event handler.
        /// Use for callback purpose
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// SqlDependency object related to this CacheEntry.  
        /// This is for unregister the SqlDependency OnChange delegate when the entry is purged from the cache.
        /// </summary>
        public SqlDependency SqlDependecy { get; set; }


        /// <summary>
        /// SqlDependency OnChange event handler.
        /// This is for unregister the SqlDependency OnChange delegate when the entry is purged from the cache.
        /// </summary>
        public OnChangeEventHandler SqlDependecyOnChangeHandler { get; set; }

    }
}
