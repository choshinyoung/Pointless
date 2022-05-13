using Discord;
using Discord.WebSocket;

namespace Pointless.Utils
{
    internal static class IUserExtension
    {
        public static string GetAvatar(this IUser user)
        {
            if (user is SocketGuildUser guildUser)
            {
                string avatar = guildUser.GetGuildAvatarUrl();

                if (avatar != null)
                {
                    return avatar;
                }
            }

            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetName(this IUser user, bool isPrivate)
        {
            return isPrivate ? user.Username : ((SocketGuildUser)user).DisplayName;
        }
    }
}
