using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Model {
    public class User {
        public int? Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public User(string username, string password, int? id = null) {
            Id = id;
            Username = username;
            PasswordHash = HashPassword(password);
        }
        private byte[] HashPassword(string password) {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return hashBytes;
        }
        public bool VeryifyPassword(string password) {
            byte[] hashBytes = PasswordHash;
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            for (int i = 0; i < 20; i++) {
                if (hashBytes[i + 16] != hash[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}
