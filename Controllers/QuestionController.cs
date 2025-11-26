using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol.Categories;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Repositories.Interfaces;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class QuestionController : BaseController
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly ILogger<QuestionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPlatformServiceFactory _platformServiceFactory;

        public QuestionController(IQuestionRepository questionRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<QuestionController> logger,
            IConfiguration configuration)
        {
            _questionRepository = questionRepository;
            _logger = logger;
            _configuration = configuration;
            _platformServiceFactory = platformServiceFactory;
        }
        public IActionResult Index()
        {
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            SetActiveMenu("Question");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetQuestions()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var (questions, totalCount) = await _questionRepository.GetPagedQuestionsAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = questions.Select(o => new
                    {
                        id = o.Id,
                        platform = GetPlatformDisplayName(o.Platform),
                        text = o.Text,
                        status=o.Status,
                        creationDate = o.CreationDate.ToString("dd.MM.yyyy HH:mm"),
                        actions = o.Id
                    })
                };

                return Json(returnObj);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> FetchTrendyolQuestions()
        {
            try
            {
                var trendyolService = _platformServiceFactory.GetTrendyolService();
                List<Question> questions = await trendyolService.GetQuestionsAsync();
                int addedCount = 0;
                int updatedCount = 0;
                foreach (var question in questions)
                {
                    var qst = await _questionRepository.GetByQuestionIdAsync(question.QuestionId);

                    if (qst != null)
                    {
                        qst.Answer = question.Answer;
                        qst.AnsweredDateMessage = question.AnsweredDateMessage;
                        qst.Text = question.Text;
                        qst.Answer = JsonConvert.SerializeObject(question.Answer);
                        qst.Status = Util.GetTrendyolQuestionStatus(question.Status);
                        await _questionRepository.UpdateAsync(qst);
                        updatedCount++;
                    }
                    else
                    {
                        await _questionRepository.CreateAsync(question);
                        addedCount++;
                    }
                }
                return Json(new
                {
                    success = true,
                    message = $"Trendyol için {addedCount} yeni soru eklendi. {updatedCount} soru güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol sorular çekilirken hata: {ex.Message}"
                });
            }
        }

        private string GetPlatformDisplayName(Platform platform)
        {
            return platform switch
            {
                Platform.Trendyol => "Trendyol",
                _ => "Bilinmeyen"
            };
        }
    }
}
