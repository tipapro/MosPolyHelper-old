namespace MosPolytechHelper.Common.Interfaces
{
    using System;
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}