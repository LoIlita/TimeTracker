using TimeTracker.Domain.Entities;

namespace TimeTracker.Domain.Validation;

public static class WorkSessionValidator
{
    public static void ValidateSession(WorkSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session), "Sesja nie może być null");
        }

        if (string.IsNullOrWhiteSpace(session.Description))
        {
            throw new ArgumentException("Opis sesji nie może być pusty", nameof(session));
        }

        ValidateStartTime(session.StartTime);
        
        if (session.EndTime.HasValue)
        {
            ValidateEndTime(session.StartTime, session.EndTime.Value);
        }
    }

    public static void ValidateStartTime(DateTime startTime)
    {
        if (startTime > DateTime.Now)
        {
            throw new ArgumentException("Czas rozpoczęcia nie może być w przyszłości", nameof(startTime));
        }
    }

    public static void ValidateEndTime(DateTime startTime, DateTime endTime)
    {
        if (endTime > DateTime.Now)
        {
            throw new ArgumentException("Czas zakończenia nie może być w przyszłości", nameof(endTime));
        }

        if (endTime <= startTime)
        {
            throw new ArgumentException("Czas zakończenia musi być późniejszy niż czas rozpoczęcia", nameof(endTime));
        }
    }
} 