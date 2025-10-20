using AttaEduSystem.Services.Services.CloudinaryModule.Commands;

namespace AttaEduSystem.Services.Services.CloudinaryModule.Invoker
{
    public class CloudinaryServiceControl
    {
        private ICommand _command;

        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        /// <summary>
        /// Async method to run commands related to Cloudinary services.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RunAsync()
        {
            if (_command is null)
            {
                throw new InvalidOperationException("Command not set.");
            }

            await _command.ExecuteAsync();
        }
    }
}
