namespace AuctionSystem.API.Helpers;


public static class AuctionGroupTracker
{

    private static readonly Dictionary<int, HashSet<int>> ConnectedUsersByAuction = new();


    private static readonly object LockObject = new();

    public static void AddUserToAuction(int productId, int userId)
    {
        lock (LockObject)
        {
            if (!ConnectedUsersByAuction.TryGetValue(productId, out var connectedUsersSet))
            {
                connectedUsersSet = new HashSet<int>();
                ConnectedUsersByAuction[productId] = connectedUsersSet;
            }

            connectedUsersSet.Add(userId);
        }
    }


    public static void RemoveUserFromAuction(int productId, int userId)
    {
        lock (LockObject)
        {
            if (ConnectedUsersByAuction.TryGetValue(productId, out var connectedUsersSet))
            {
                connectedUsersSet.Remove(userId);

                // Clean up empty entries
                if (connectedUsersSet.Count == 0)
                {
                    ConnectedUsersByAuction.Remove(productId);
                }
            }
        }
    }

    public static int GetConnectedUserCount(int productId)
    {
        lock (LockObject)
        {
            return ConnectedUsersByAuction.TryGetValue(productId, out var connectedUsersSet)
                ? connectedUsersSet.Count
                : 0;
        }
    }
}
