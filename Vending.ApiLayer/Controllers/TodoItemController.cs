using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Vending.DataAccessLayer.Concrete;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemController : ControllerBase
    {
        private readonly VendingContext _context;

        public TodoItemController(VendingContext context)
        {
            _context = context;
        }


        // GET: api/todos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/todos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // POST: api/todos
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // PUT: api/todos/5
        // PUT: api/todos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            var existingTodoItem = await _context.TodoItems.FindAsync(id);
            if (existingTodoItem == null)
            {
                return NotFound();
            }

            // Update the Completed property to true
            existingTodoItem.Completed = true;

            // Optionally, you can update other properties from the incoming todoItem
            // e.g., existingTodoItem.Title = todoItem.Title; // If you want to update the title too

            // Set the state to modified
            _context.Entry(existingTodoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // PATCH: api/todos/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTodoItem(int id)
        {
            // Attempt to find the TodoItem with the given id
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound(); // Return 404 if not found
            }

            // Toggle the Completed property
            todoItem.Completed = !todoItem.Completed;

            try
            {
                // Mark the entity as modified
                _context.Entry(todoItem).State = EntityState.Modified;

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle the concurrency error
                if (!TodoItemExists(id))
                {
                    return NotFound(); // Item no longer exists
                }
                else
                {
                    throw; // Re-throw the exception for other handling
                }
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework here)
                Console.WriteLine($"Error updating TodoItem: {ex.Message}");
                return StatusCode(500, "Internal server error"); // Return 500 for any other errors
            }

            return NoContent(); // Return 204 No Content if successful
        }

        // Helper method to check if the TodoItem exists
 
        // DELETE: api/todos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoItemExists(int id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
