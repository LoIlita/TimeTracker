using System;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Domain.Validation;

public static class ActivitySessionValidator
{
    public static void ValidateSession(ActivitySession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        // Najpierw wykonujemy podstawową walidację WorkSession
        WorkSessionValidator.ValidateSession(session);

        if (session.ProjectId == Guid.Empty)
            throw new ValidationException("Sesja musi być przypisana do projektu.");

        if (string.IsNullOrWhiteSpace(session.Category))
            throw new ValidationException("Kategoria sesji nie może być pusta.");

        if (session.Category.Length > 100)
            throw new ValidationException("Kategoria nie może być dłuższa niż 100 znaków.");

        if (session.Notes?.Length > 2000)
            throw new ValidationException("Notatki nie mogą być dłuższe niż 2000 znaków.");

        if (session.Rating.HasValue && (session.Rating.Value < 1 || session.Rating.Value > 5))
            throw new ValidationException("Ocena musi być w zakresie 1-5.");
    }
} 