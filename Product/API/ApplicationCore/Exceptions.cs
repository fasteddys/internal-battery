using System;
using System.Diagnostics;
namespace UpDiddyApi.ApplicationCore.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class MaximumReachedException : Exception
    {
        public MaximumReachedException() : base() { }
        public MaximumReachedException(string message) : base(message)
        {

        }
        public MaximumReachedException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException() : base() { }
        public AlreadyExistsException(string message) : base(message)
        {

        }
        public AlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }
    }

    public class ExpiredJobException : Exception
    {
        public ExpiredJobException() : base() { }
        public ExpiredJobException(string message) : base(message)
        {

        }
        public ExpiredJobException(string message, Exception innerException)
        : base(message, innerException) { }
    }

    public class FailedValidationException : Exception
    {
        public FailedValidationException() : base() { }
        public FailedValidationException(string message) : base(message)
        {

        }
        public FailedValidationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class FileSizeExceedsLimit : Exception
    {
        public FileSizeExceedsLimit() : base() { }
        public FileSizeExceedsLimit(string message) : base(message)
        {

        }
        public FileSizeExceedsLimit(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class JobPostingCreation : Exception
    {
        public JobPostingCreation() : base() { }
        public JobPostingCreation(string message) : base(message)
        {

        }
        public JobPostingCreation(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class JobPostingUpdate : Exception
    {
        public JobPostingUpdate() : base() { }
        public JobPostingUpdate(string message) : base(message)
        {

        }
        public JobPostingUpdate(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class TraitifyException : Exception
    {
        public TraitifyException() : base() { }
        public TraitifyException(string message) : base(message)
        {

        }
        public TraitifyException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class OfferException : Exception
    {
        public OfferException() : base() { }
        public OfferException(string message) : base(message)
        {

        }
        public OfferException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}