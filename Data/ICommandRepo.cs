using MinAPI.Models;

namespace MinAPI.Data
{
    public interface ICommandRepo
    {
        Task SaveChanges();
        Task<Command?> GetCommandById(int id); 
        Task<IEnumerable<Command>> GetAllCommands();
        Task CreateCommand(Command command);

        void DeleteCommand(Command command);    
    }
}