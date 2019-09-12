namespace MosPolytechHelper.Features.Common
{
    public struct VmMessage
    {
        private readonly object[] subMessages;

        public int Count =>
            this.subMessages == null ? 0 : this.subMessages.Length;

        public VmMessage(params object[] subMessages)
        {
            this.subMessages = subMessages;
        }

        public object Get(int index = 0) =>
            index >= 0 && index < this.subMessages.Length ? this.subMessages[index] : null;

        public object this[int index] =>
            Get(index);
    }
}