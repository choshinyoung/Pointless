using Discord;
using Discord.Interactions;

namespace Pointless.Commands.Admins
{
    [Group("관리자", "관리자 전용 커맨드입니다")]
    [EnabledInDm(false)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public partial class AdminCommands : InteractionModuleBase<SocketInteractionContext>
    {

    }
}
