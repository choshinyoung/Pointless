using Discord.Interactions;

namespace Pointless.Modals
{
    public class TotoCustomPointModal : IModal
    {
        public string Title => "예측 포인트 직접 지정하기";

        [InputLabel("포인트")]
        [ModalTextInput("point")]
        public string Point { get; set; }
    }
}
