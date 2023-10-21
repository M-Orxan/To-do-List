using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ToDoList.DAL;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        //Index
        public async Task<IActionResult> Index(int currentPage=1)
        {
            ViewBag.CurrentPage = currentPage;
            
            int taskPerPage = 5;
            int taskCount=await _db.Tasks.Where(x=>x.IsDeactive==false).CountAsync();
            ViewBag.PageCount = Math.Ceiling((decimal)taskCount / taskPerPage);
            List<Models.Task> tasks = await _db.Tasks.Where(x => x.IsDeactive == false).Skip((currentPage-1)*taskPerPage).Take(taskPerPage).ToListAsync();
            return View(tasks);
        }

        //Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]

        public async Task<IActionResult> Create(Models.Task task)
        {
            task.Title = task.Title?.Trim();
            bool isExists = await _db.Tasks.Where(x => x.IsDeactive == false).AnyAsync(x => x.Title == task.Title);

            if (isExists)
            {
                ModelState.AddModelError("Title", "There is already note under this title. Please change the title");
                return View(task);
            }

            if (task.Deadline < DateTime.Now)
            {
                ModelState.AddModelError("Deadline", "Deadline can not be earlier than current time");
                return View(task);

            }

            //task.Title = task.Title?.Trim();
            task.Created = DateTime.Now;
            task.InProgress = true;
            task.IsDeactive = false;


            await _db.Tasks.AddAsync(task);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        //Delete
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Models.Task? task = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                return BadRequest();
            }

            task.IsDeactive = true;

            await _db.SaveChangesAsync();


            return RedirectToAction("Index");
        }

        //Detail
        public async Task<IActionResult> Detail(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Models.Task? task = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                return BadRequest();
            }

            return View(task);
        }

        //Update
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Models.Task? task = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                return BadRequest();
            }


            return View(task);
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]

        public async Task<IActionResult> Update(int id, Models.Task task)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Models.Task? dbTask = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
            if (dbTask == null)
            {
                return BadRequest();
            }

            task.Title = task.Title?.Trim();
            bool isExists = await _db.Tasks.Where(x => x.IsDeactive == false).AnyAsync(x => x.Title == task.Title && x.Id != id);

            if (isExists)
            {
                ModelState.AddModelError("Title", "There is already note under this title. Please change the title");
                return View(task);
            }



            if (task.Deadline < DateTime.Now && task.InProgress == true)
            {
                ModelState.AddModelError("Deadline", "Deadline can not be earlier than current time");
                return View(task);

            }

           

            if (task.InProgress == false && task.Completed == false)
            {
                ModelState.AddModelError("InProgress", "Status must be either InProgress or Completed");
                return View(task);
            }

            dbTask.Title = task.Title;
            dbTask.Description = task.Description;
            dbTask.Deadline = task.Deadline;
            dbTask.Completed = task.Completed;
            dbTask.InProgress = task.InProgress;

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}