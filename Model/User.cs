using System;
using System.Security.Cryptography;

namespace Model {
    public class User {
        public int? Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public string SessionCookie { get; set; }

        public User(string username, string password, string email)
            : this(username, password, email, null) {

        }
        public User(string username, string password, string email, byte[] sessionCookie)
            : this(username, password, email, sessionCookie, null) {

        }
        public User(string username, string password, string email, byte[] sessionCookie, int? id)
            : this(username, HashPassword(password), email, sessionCookie, id) {

        }
        public User(string username, byte[] password, string email, byte[] sessionCookie, int? id)
            : this(username, password, email, sessionCookie == null ? null : Convert.ToBase64String(sessionCookie), id) {

        }
        public User(string username, byte[] passwordHash, string email, string sessionCookie, int? id) {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            SessionCookie = sessionCookie;
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
        public User NewSessionCookie() {
            Random rng = new Random();
            byte[] bytes = new byte[20];
            rng.NextBytes(bytes);
            SessionCookie = Convert.ToBase64String(bytes);
            return this;
        }
    }
}
