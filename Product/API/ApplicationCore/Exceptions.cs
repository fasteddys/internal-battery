using System;
using System.Diagnostics;
namespace UpDiddyApi.ApplicationCore.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, NotFoundException innerException)
            : base(message, innerException) { }
    }

    public class MaximumReachedException : Exception
    {
        public MaximumReachedException() : base() { }
        public MaximumReachedException(string message) : base(message)
        {

        }
        public MaximumReachedException(string message, MaximumReachedException innerException)
            : base(message, innerException) { }
    }

    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException() : base() { }
        public AlreadyExistsException(string message) : base(message)
        {

        }
        public AlreadyExistsException(string message, AlreadyExistsException innerException)
        : base(message, innerException) { }
    }

    public class ExpiredJobException : Exception
    {
        public ExpiredJobException() : base() { }
        public ExpiredJobException(string message) : base(message)
        {

        }
        public ExpiredJobException(string message, ExpiredJobException innerException)
        : base(message, innerException) { }
    }
    public class FailedValidationException : Exception
    {
        public FailedValidationException() : base() { }
        public FailedValidationException(string message) : base(message)
        {

        }
        public FailedValidationException(string message, FailedValidationException innerException)
            : base(message, innerException) { }
    }


    public class FileSizeExceedsLimit : Exception
    {
        public FileSizeExceedsLimit() : base() { }
        public FileSizeExceedsLimit(string message) : base(message)
        {

        }
        public FileSizeExceedsLimit(string message, FileSizeExceedsLimit innerException)
            : base(message, innerException) { }
    }


    public class JobPostingCreation : Exception
    {
        public JobPostingCreation() : base() { }
        public JobPostingCreation(string message) : base(message)
        {

        }
        public JobPostingCreation(string message, FileSizeExceedsLimit innerException)
            : base(message, innerException) { }
    }


    public class JobPostingUpdate : Exception
    {
        public JobPostingUpdate() : base() { }
        public JobPostingUpdate(string message) : base(message)
        {

        }
        public JobPostingUpdate(string message, FileSizeExceedsLimit innerException)
            : base(message, innerException) { }
    }

    public class TraitifyException : Exception
    {
        public TraitifyException() : base() { }
        public TraitifyException(string message) : base(message)
        {

        }
        public TraitifyException(string message, TraitifyException innerException)
            : base(message, innerException) { }
    }

    public class OfferException : Exception
    {
        public OfferException() : base() { }
        public OfferException(string message) : base(message)
        {

        }
        public OfferException(string message, OfferException innerException)
            : base(message, innerException) { }
    }

    public class InsufficientPermissionException : Exception
    {
        public InsufficientPermissionException() : base() { }
        public InsufficientPermissionException(string message) : base(message)
        {

        }
        public InsufficientPermissionException(string message, InsufficientPermissionException innerException)
            : base(message, innerException) { }
    }
}


