using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QWest.DataAccess.Tests {
    public static class Utils {
        public static void CleanUp(bool deleteGeopoliticalData = false) {
            List<string> toDelete = new string[] {
                "progress_maps_locations",
                "users_friendships",
                "users_friendship_requests",
                "users_groups",
                "posts_images",
                "posts",
                "groups",
                "password_reset_tokens",
                "users",
                "images",
                "progress_maps",
            }.ToList();
            if (deleteGeopoliticalData) {
                toDelete.Add("geopolitical_location");
            }
            ConnectionWrapper.Instance.Use(string.Join(" ", toDelete.Select(x => $"DELETE FROM {x};")), stmt => stmt.ExecuteNonQueryAsync()).Wait();
        }
    }
}
