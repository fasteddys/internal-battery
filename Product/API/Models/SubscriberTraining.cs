using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberTraining : BaseModel
    {
        public int SubscriberTrainingId { get; set; }

        public Guid SubscriberTrainingGuid { get; set; }

        public int TrainingTypeId { get; set; }
        public virtual TrainingType TrainingType { get; set; }

        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }

        [Column(TypeName = "Varchar(150)"), MaxLength(150)]
        public string TrainingInstitution { get; set; }

        [Column(TypeName = "Varchar(150)"), MaxLength(150)]
        public string TrainingName { get; set; }

        [Column(TypeName = "SmallInt")]
        public short? RelevantYear { get; set; }



    }
}
