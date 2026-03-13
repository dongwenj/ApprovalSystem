using FluentValidation;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Domain.Constants;
using System.Globalization;

namespace MyWebApi.Application.Validator
{
    public class ApplicationFormAdd_ReqValidator : AbstractValidator<ApplicationFormAdd_Req>
    {
        public ApplicationFormAdd_ReqValidator()
        {
            ValidatorOptions.Global.LanguageManager.Enabled = true;
            ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("zh-TW");

            RuleFor(x => x.ApplicationDate.Date)
                .NotEmpty().WithMessage(string.Format(ErrorMessages.NotEmpty, "申請日期"))
                .LessThanOrEqualTo(DateTime.Today).WithMessage(string.Format(ErrorMessages.DateMustLongerThanToday, "申請日期"));

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage(string.Format(ErrorMessages.NotEmpty, "申請事由"))
                .MaximumLength(50).WithMessage(string.Format(ErrorMessages.StringMustShorter, "申請事由", "50"));

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage(string.Format(ErrorMessages.NotEmpty, "申請明細"));

            RuleForEach(x => x.Items).SetValidator(new ItemValidator());
        }
    }

    public class ItemValidator : AbstractValidator<ApplicationFormAdd_Req.Item>
    {
        public ItemValidator()
        {
            ValidatorOptions.Global.LanguageManager.Enabled = true;
            ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("zh-TW");

            RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.NotEmpty, "品名"))
            .MaximumLength(100).WithMessage(string.Format(ErrorMessages.StringMustShorter, "品名", "100"));

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage(string.Format(ErrorMessages.NotEmpty, "單位"))
                .MaximumLength(5).WithMessage(string.Format(ErrorMessages.StringMustShorter, "單位", "5"));

            RuleFor(x => x.Quantity)
                .NotEmpty().WithMessage(string.Format(ErrorMessages.NotEmpty, "數量"))
                .GreaterThan(0).WithMessage(string.Format(ErrorMessages.NumberMustGreater, "數量", "0"));

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage(string.Format(ErrorMessages.NotEmpty, "價格"))
                .GreaterThan(0).WithMessage(string.Format(ErrorMessages.NumberMustGreater, "數量", "0"));
        }
    }
}

