namespace SsitEngine.PureMVC.Interfaces
{
    /// <summary>
    ///     The interface definition for a PureMVC Command.
    /// </summary>
    /// <seealso cref="INotification" />
    public interface ICommand
    {
        /// <summary>
        ///     Execute the <c>ICommand</c>'s logic to handle a given <c>INotification</c>.
        /// </summary>
        /// <param name="notification">an <c>INotification</c> to handle.</param>
        void Execute( INotification notification );
    }
}