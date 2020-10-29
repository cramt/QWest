﻿using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace QWest.DataAcess {
    public static partial class DAO {
        // friendship is magic
        public interface IFriendship {
            Task<List<User>> GetByUser(User user);
            Task<List<User>> GetByUser(int userId);
            Task AddFriendRequest(User from, User to);
            Task AddFriendRequest(User from, int to);
            Task AddFriendRequest(int from, User to);
            Task AddFriendRequest(int from, int to);
            Task<List<User>> GetFriendRequests(User user);
            Task<List<User>> GetFriendRequests(int userId);
            Task AcceptFriendRequest(User from, User to);
            Task AcceptFriendRequest(User from, int to);
            Task AcceptFriendRequest(int from, User to);
            Task AcceptFriendRequest(int from, int to);
        }
        public static IFriendship Friendship { get; set; } = new Mssql.FriendshipImpl(ConnectionWrapper.Instance);
    }
}
