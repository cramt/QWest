using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Utilities;

namespace Model {
    [Serializable]
    public class User {
        public int? Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public byte[] PasswordHash { get; set; }
        [JsonIgnore]
        public string SessionCookie { get; set; }
        [JsonIgnore]
        public ProgressMap ProgressMap { get; set; }
        public List<User> Friends { get; set; }
        public int? ProfilePicture { get; set; }

        public User(string username, string password, string email)
            : this(username, password, email, "") {

        }
        public User(string username, string password, string email, string description) :
            this(username, password, email, description, null) { }
        public User(string username, string password, string email, string description, byte[] sessionCookie)
            : this(username, password, email, description, sessionCookie, null) {

        }
        public User(string username, string password, string email, string description, byte[] sessionCookie, int? id)
            : this(username, HashPassword(password), email, description, sessionCookie, id) {

        }
        public User(string username, byte[] password, string email, string description, byte[] sessionCookie, int? id)
            : this(username, password, email, description, sessionCookie.MapValue(Convert.ToBase64String), id) {

        }
        public User(string username, byte[] passwordHash, string email, string description, string sessionCookie, int? id) {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            SessionCookie = sessionCookie;
            ProgressMap = null;
            Description = description;
        }
        private static byte[] HashPassword(string password) {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return hashBytes;
        }
        public bool VerifyPassword(string password) {
            byte[] hashBytes = PasswordHash;
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);
            for (int i = 0; i < 20; i++) {
                if (hashBytes[i + 16] != hash[i]) {
                    return false;
                }
            }
            return true;
        }

        public User NewPassword(string password) {
            PasswordHash = HashPassword(password);
            return this;
        }

        public User ResetSessionCookie() {
            SessionCookie = null;
            return this;
        }

        [JsonConstructor]
        public User() {

        }
    }
}
