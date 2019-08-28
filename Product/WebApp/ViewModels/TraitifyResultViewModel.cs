using UpDiddyLib.Dto;
using System.Collections.Generic;
using com.traitify.net.TraitifyLibrary;

namespace UpDiddy.ViewModels
{
    public class TraitifyResultViewModel 
    {
        public string AssesmentId {get;set;}
        public List<AssessmentPersonalityTrait> AssessmentPersonalityTraits {get;set;}
        public AssessmentPersonalityTypes AssessmentPersonalityTypes {get;set;}
    }
}
