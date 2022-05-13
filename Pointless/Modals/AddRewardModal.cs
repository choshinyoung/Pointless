using Discord;
using Discord.Interactions;

namespace Pointless.Modals
{
    public class AddRewardModal : IModal
    {
        public string Title => "리워드 추가";

        [InputLabel("이름")]
        [ModalTextInput("name")]
        public string Name { get; set; }

        [InputLabel("설명")]
        [ModalTextInput("description", TextInputStyle.Paragraph)]
        public string Description { get; set; }

        [InputLabel("포인트")]
        [ModalTextInput("point")]
        public string Point { get; set; }
    }
}
