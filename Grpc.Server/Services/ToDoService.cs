using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Server.Context;
using Grpc.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Grpc.Server.Services;

public class ToDoService : ToDo.ToDoBase
{
    //private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;

    public ToDoService(/*ILogger logger,*/ AppDbContext dbContext)
    {
        //_logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDoItem(CreateToDoRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Description))
        {
            throw new RpcException(GenerateStatus(StatusCode.InvalidArgument, "Title and Description is required"));
        }
        //Add auto mapper
        var newTodo = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(newTodo);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = newTodo.Id,
            TodoStatus = newTodo.TodoStatus
        });
    }

    private static Status GenerateStatus(StatusCode code, string message)
    {
        return new Status(code,message);
    }

    public override async Task<UpdateToDoResponse> UpdateToDoItem(UpdateToDoRequest request, ServerCallContext context)
    {
        var toBeUpdated = await _dbContext.ToDoItems.FirstOrDefaultAsync(s => s.Id == request.Id);
        
        if(toBeUpdated == null)
            throw new RpcException(GenerateStatus(StatusCode.NotFound, "Record not found"));

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            throw new RpcException((GenerateStatus(StatusCode.InvalidArgument, "Please provide valid arguments.")));

        toBeUpdated.Title = request.Title;
        toBeUpdated.Description = request.Description;
        toBeUpdated.TodoStatus = request.TodoStatus;
        
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse
        {
            Id = toBeUpdated.Id
        });

    }

    public override async Task<DeleteToDoResponse> DeleteToDoItem(DeleteToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(GenerateStatus(StatusCode.InvalidArgument, "Id cannot be be 0 or negative."));
        }

        var toBeDeleted = await _dbContext.ToDoItems.FirstOrDefaultAsync(s => s.Id == request.Id);
        
        if(toBeDeleted == null)
            throw new RpcException(GenerateStatus(StatusCode.NotFound, "Record not found"));

        _dbContext.Remove(toBeDeleted);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse
        {
            Id = toBeDeleted.Id
        });
    }

    public override async Task<TodoItemResponse> GetToDoItem(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(GenerateStatus(StatusCode.InvalidArgument, "Id cannot be be 0 or negative."));
        }

        var returnItem = 
            await _dbContext.ToDoItems
                .Where(w => w.Id == request.Id)
                .Select(s => 
                    new TodoItemResponse
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        TodoStatus = s.TodoStatus
                    })
                .FirstOrDefaultAsync();
        
        if(returnItem != null)
            return await Task.FromResult(returnItem);

        throw new RpcException(GenerateStatus(StatusCode.NotFound, "No item found."));
    }

    public override async Task<GetAllTodos> GetToDoItems(Empty request, ServerCallContext context)
    {
        var allRecords =  
            await _dbContext.ToDoItems
            .Select(
                s => new TodoItemResponse
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        TodoStatus = s.TodoStatus
                    }
                )
            .ToListAsync();
        
        return await Task.FromResult(new GetAllTodos {Items = {allRecords}});

    }
}