using ListAPI.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Globalization;
using BCrypt;
using Microsoft.EntityFrameworkCore;

namespace ListAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : Controller
    {
        string salt = BCrypt.Net.BCrypt.GenerateSalt();

        DataBaseContext db = new DataBaseContext();


        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }


        ////////////////////////////

        [HttpPost(template: "Registr")]  //Регистрация
        public async Task<ActionResult<UserT>> RegistrUser(UserT UserGet)
        {
            if (db.UserTs.FirstOrDefault(z => z.Username == UserGet.Username) != null)
            {
                return BadRequest("Пользователь с таким именем уже существует");
            }
            else if (UserGet.Username == null || UserGet.PasswordHash == null)
            {
                return BadRequest("Заполните все поля");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(UserGet.PasswordHash, salt);

            UserGet.PasswordHash = hashedPassword;

            db.UserTs.Add(UserGet);
            db.SaveChanges();
            return Ok(UserGet);
        }



        ////////////////////////////

        [HttpGet("Auth/{Username}/{Password}")] // Авторизация
        public async Task<ActionResult<UserT>> AuthUser(string Username, string Password)
        {
            if (Username == null || Password == null)
            {
                return BadRequest("Заполните поля");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password, salt);

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(Password, hashedPassword);

            var GetUser = db.UserTs.FirstOrDefault(z => z.Username == Username && isPasswordCorrect == true);

            if (GetUser == null)
            {
                return NotFound("Не верные данные");
            }

            return Ok(GetUser);
        }



        ////////////////////////////

        [HttpGet("User/{Username}")] // Получение инфорамации о пользователе
        public async Task<ActionResult<UserT>> GetUserinfo(string Username)
        {
            if (Username == "")
            {
                return BadRequest("Укажите имя пользователя");
            }

            UserT GetUser = db.UserTs.FirstOrDefault(z => z.Username == Username);

            if (GetUser == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(GetUser);
        }



        ////////////////////////////

        [HttpGet("Task/{UserName}")] // Получение списка задач
        public async Task<ActionResult<TaskT>> GetTask(string UserName)
        {
            if (UserName == null || UserName == "")
            {
                return BadRequest("Заполните поле");
            }

            int UserNameId = db.UserTs.FirstOrDefault(z => z.Username == UserName).Id;

            var GetTask = db.TaskTs.Where(z => z.UserId == UserNameId);

            if (GetTask == null)
            {
                return NotFound("Задачи не найдены");
            }

            return Ok(GetTask);
        }

        [HttpGet("TaskGet/{Id}")]
        public async Task<ActionResult<TaskT>> GetTaskId(int Id)
        {
            if (Id == null)
            {
                return BadRequest("Поле Id не заполнено");
            }

            TaskT GetTaskId = db.TaskTs.FirstOrDefault(z => z.Id == Id);

            if (GetTaskId == null)
            {
                return NotFound("Задача не найдена");
            }

            return Ok(GetTaskId);

        }



        ////////////////////////////

        [HttpPost(template: "CreateTask")] // Создание новой задачи
        public async Task<ActionResult<TaskT>> CreateTask(TaskT TaskNew)
        {
            UserT NewUserSet = db.UserTs.FirstOrDefault(z => z.Id == TaskNew.UserId);

            TaskT NewTaskSet = new TaskT
            {
                UserId = TaskNew.UserId,
                Name = TaskNew.Name,
                Description = TaskNew.Description,
                Deadline = TaskNew.Deadline,
                Done = TaskNew.Done,
                Priority = TaskNew.Priority,
                User = TaskNew.User
            };

            NewTaskSet.User = NewUserSet;

            if (TaskNew.Name == "" || TaskNew.Description == "" || TaskNew.Deadline == null || TaskNew.Priority == "")
            {
                return BadRequest("Заполните все поля");
            }
            else if (TaskNew.Done == null || TaskNew.Name == null || TaskNew.Description == null || TaskNew.Deadline == null || TaskNew.Priority == null)
            {
                return BadRequest("Заполните все поля");
            }
            try
            {
                db.Add(NewTaskSet);
                db.SaveChanges();
                return Ok(NewTaskSet);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Произошла ошибка: " + ex.Message);
                return BadRequest();
            }
        }



        ////////////////////////////

        [HttpPut(template: "UpdateTask")] // Обновление задачи
        public async Task<ActionResult<TaskT>> UpdateTask(TaskT TaskGet)
        {
            var NewTask = db.TaskTs.FirstOrDefault(z => z.Id == TaskGet.Id);
            UserT NewUserSet = db.UserTs.FirstOrDefault(z => z.Id == TaskGet.UserId);

            if (TaskGet == null)
            {
                return BadRequest("Задача не найдена");
            }

            if (TaskGet.Name == "" || TaskGet.Description == "" || TaskGet.Deadline == null || TaskGet.Priority == "")
            {
                return BadRequest("Заполните все поля");
            }
            else if (TaskGet.Done == null || TaskGet.Name == null || TaskGet.Description == null || TaskGet.Deadline == null || TaskGet.Priority == null)
            {
                return BadRequest("Заполните все поля");
            }
            
            NewTask.UserId = TaskGet.UserId;
            NewTask.Name = TaskGet.Name;
            NewTask.Description = TaskGet.Description;
            NewTask.Deadline = TaskGet.Deadline;
            NewTask.Done = TaskGet.Done;
            NewTask.Priority = TaskGet.Priority;
            NewTask.User = NewUserSet;

            db.Update(NewTask);

            db.SaveChanges();

            return Ok(NewTask);
        }



        ////////////////////////////

        [HttpDelete("Delete/{IdTask}")] // Удаление задачи
        public async Task<ActionResult<TaskT>> DeleteTask(int IdTask)
        {
            if (IdTask == null)
            {
                return BadRequest("Заполните поля");
            }
            var GetTask = db.TaskTs.FirstOrDefault(z => z.Id == IdTask);

            if (GetTask == null)
            {
                return NotFound("Задача не найдена");
            }

            db.TaskTs.Remove(GetTask);
            db.SaveChanges();
            return Ok();

        }



        ////////////////////////////

        [HttpGet(template: "Test")]
        public async Task<ActionResult> Test()
        {
            return Ok("Усппешно");

        }
    }
}
