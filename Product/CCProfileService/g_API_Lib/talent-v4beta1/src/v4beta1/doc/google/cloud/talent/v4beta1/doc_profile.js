// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Note: this file is purely for documentation. Any contents are not expected
// to be loaded as the JS file.

/**
 * A resource that represents the profile for a job candidate (also referred to
 * as a "single-source profile"). A profile belongs to a Company, which is
 * the company/organization that owns the profile.
 *
 * @property {string} name
 *   Required during profile update.
 *
 *   Resource name assigned to a profile by the API.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}",
 *   for example, "projects/api-test-project/tenants/foo/profiles/bar".
 *
 * @property {string} externalId
 *   Optional.
 *
 *   Profile's id in client system if available.
 *
 *   The maximum number of bytes allowed is 100.
 *
 * @property {string} externalSystem
 *   Optional.
 *
 *   The system of record / source system / operating company from which this
 *   profile is extracted. This is intended to be populated where the same job
 *   might be represented differently to different recruiters.
 *
 *   From a training perspective it is important to maintain a separation of
 *   data that best represents the information an individual recruiter had
 *   access to during the search and placement process.
 *
 * @property {string} source
 *   Optional.
 *
 *   The source description indicating where the profile is acquired.
 *
 *   For example, if a candidate profile is acquired from a resume, the user can
 *   input "resume" here to indicate the source.
 *
 *   The maximum number of bytes allowed is 100.
 *
 * @property {string} uri
 *   Optional.
 *
 *   The URI set by clients that links to this profile's client-side copy.
 *
 *   The maximum number of bytes allowed is 4000.
 *
 * @property {string} sourceNote
 *   Optional.
 *
 *   Other information about this source.
 *
 * @property {string} groupId
 *   Optional.
 *
 *   The cluster id of the profile to associate with other profile(s) for the
 *   same candidate.
 *
 *   A random UUID is assigned if group_id isn't provided. To ensure
 *   global uniqueness, customized group_id isn't supported. If
 *   group_id is set, there must be at least one other profile with the
 *   same system generated group_id, otherwise an error is thrown.
 *
 *   This is used to link multiple profiles to the same candidate. For example,
 *   a client has a candidate with two profiles, where one was created recently
 *   and the other one was created 5 years ago. These two profiles may be very
 *   different. The clients can create the first profile and get a generated
 *   group_id, and assign it when the second profile is created,
 *   indicating these two profiles are referring to the same candidate.
 *
 * @property {Object.<string, Object>} fieldUpdateMetadata
 *   Output only. A history of when the candidate's information was updated or
 *   reviewed. This is important for tracking situations where data was reviewed
 *   and updated, but the data value did not change (for example, for tracking
 *   field-level freshness).
 *
 * @property {Object} isHirable
 *   Optional.
 *
 *   Indicates the hirable status of the candidate.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object} createTime
 *   Optional.
 *
 *   The timestamp when the profile was first created at this source.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} updateTime
 *   Optional.
 *
 *   The timestamp when the profile was last updated at this source.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} resumeHrxml
 *   Optional.
 *
 *   The profile contents in HR-XML format.
 *   See http://schemas.liquid-technologies.com/hr-xml/2007-04-15/ for more
 *   information about Human Resources XML.
 *
 *   Users can create a profile with only resume_hrxml field. For example,
 *   the API parses the resume_hrxml and creates a profile with all
 *   structured fields populated, for example. EmploymentRecord,
 *   EducationRecord, and so on. An error is thrown if the resume_hrxml
 *   can't be parsed.
 *
 *   If the resume_hrxml is provided during profile creation or update,
 *   any other structured data provided in the profile is ignored. The
 *   API populates these fields by parsing the HR-XML.
 *
 * @property {string[]} primaryResponsibilities
 *   Optional.
 *
 *   A list of current job responsibilities. This should be populated when
 *   there is no knowledge of where a candidate is currently working
 *   (for example, indicated by the employmentRecord field) but the
 *   candidate provides their current responsibilities.
 *
 * @property {string[]} citizenships
 *   Optional.
 *
 *   Citizenship/s of the candidate.
 *
 * @property {string[]} workAuthorizations
 *   Optional.
 *
 *   List of values where the candidate is authorized to work (e.g. H1b,
 *   citizen, green card).
 *
 * @property {string[]} employeeTypes
 *   Optional.
 *
 *   A list of employee types (e.g. W2, Vendor, Unknown) which the employee is
 *   allowed to be hired against.
 *
 * @property {string} languageCode
 *   Optional.
 *
 *   The language in which the profile information is provided.
 *   This field is distinct from the languages the candidate is fluent in,
 *   which should be provided as a skill.
 *
 * @property {string} qualificationSummary
 *   Optional.
 *
 *   A list or paragraph displaying a range of the persons' most impressive
 *   achievements.
 *
 * @property {Object[]} personNames
 *   Optional.
 *
 *   The names of the candidate this profile references.
 *
 *   Currently only one person name is supported.
 *
 *   This object should have the same structure as [PersonName]{@link google.cloud.talent.v4beta1.PersonName}
 *
 * @property {Object[]} addresses
 *   Optional.
 *
 *   The candidate's postal addresses.
 *
 *   This object should have the same structure as [Address]{@link google.cloud.talent.v4beta1.Address}
 *
 * @property {Object[]} emailAddresses
 *   Optional.
 *
 *   The candidate's email addresses.
 *
 *   This object should have the same structure as [Email]{@link google.cloud.talent.v4beta1.Email}
 *
 * @property {Object[]} phoneNumbers
 *   Optional.
 *
 *   The candidate's phone number(s).
 *
 *   This object should have the same structure as [Phone]{@link google.cloud.talent.v4beta1.Phone}
 *
 * @property {Object[]} personalUris
 *   Optional.
 *
 *   The candidate's personal URIs.
 *
 *   This object should have the same structure as [PersonalUri]{@link google.cloud.talent.v4beta1.PersonalUri}
 *
 * @property {Object[]} additionalContactInfo
 *   Optional.
 *
 *   Available contact information besides addresses, email_addresses,
 *   phone_numbers and personal_uris. For example, Hang-out, Skype.
 *
 *   This object should have the same structure as [AdditionalContactInfo]{@link google.cloud.talent.v4beta1.AdditionalContactInfo}
 *
 * @property {Object[]} employmentRecords
 *   Optional.
 *
 *   The employment history records of the candidate. It's highly recommended
 *   to input this information as accurately as possible to help improve search
 *   quality. Here are some recommendations:
 *
 *   * Specify the start and end dates of the employment records.
 *   * List different employment types separately, no matter how minor the
 *   change is.
 *   For example, only job title is changed from "software engineer" to "senior
 *   software engineer".
 *   * Provide EmploymentRecord.is_current for the current employment if
 *   possible. If not, it's inferred from user inputs.
 *
 *   This object should have the same structure as [EmploymentRecord]{@link google.cloud.talent.v4beta1.EmploymentRecord}
 *
 * @property {Object[]} educationRecords
 *   Optional.
 *
 *   The education history record of the candidate. It's highly recommended to
 *   input this information as accurately as possible to help improve search
 *   quality. Here are some recommendations:
 *
 *   * Specify the start and end dates of the education records.
 *   * List each education type separately, no matter how minor the change is.
 *   For example, the profile contains the education experience from the same
 *   school but different degrees.
 *   * Provide EducationRecord.is_current for the current education if
 *   possible. If not, it's inferred from user inputs.
 *
 *   This object should have the same structure as [EducationRecord]{@link google.cloud.talent.v4beta1.EducationRecord}
 *
 * @property {Object[]} skills
 *   Optional.
 *
 *   The skill set of the candidate. It's highly recommended to provide as
 *   much information as possible to help improve the search quality.
 *
 *   This object should have the same structure as [Skill]{@link google.cloud.talent.v4beta1.Skill}
 *
 * @property {Object[]} languageFluencies
 *   Optional.
 *
 *   The candidate's skill set in written and spoken languages
 *
 *   This object should have the same structure as [LanguageFluency]{@link google.cloud.talent.v4beta1.LanguageFluency}
 *
 * @property {Object[]} activities
 *   Optional.
 *
 *   The individual or collaborative activities which the candidate has
 *   participated in, for example, open-source projects, class assignments that
 *   aren't listed in employment_records.
 *
 *   This object should have the same structure as [Activity]{@link google.cloud.talent.v4beta1.Activity}
 *
 * @property {Object[]} publications
 *   Optional.
 *
 *   The publications published by the candidate.
 *
 *   This object should have the same structure as [Publication]{@link google.cloud.talent.v4beta1.Publication}
 *
 * @property {Object[]} patents
 *   Optional.
 *
 *   The patents acquired by the candidate.
 *
 *   This object should have the same structure as [Patent]{@link google.cloud.talent.v4beta1.Patent}
 *
 * @property {Object[]} certifications
 *   Optional.
 *
 *   The certifications acquired by the candidate.
 *
 *   This object should have the same structure as [Certification]{@link google.cloud.talent.v4beta1.Certification}
 *
 * @property {string[]} applications
 *   Output only. The resource names of the candidate's applications.
 *
 * @property {string[]} assignments
 *   Output only. The resource names of the candidate's assignments.
 *
 * @property {Object[]} recruitingNotes
 *   Optional.
 *
 *   The recruiting notes added for the candidate.
 *
 *   For example, the recruiter can add some unstructured comments for this
 *   candidate like "this candidate also has experiences in volunteer work".
 *
 *   This object should have the same structure as [RecruitingNote]{@link google.cloud.talent.v4beta1.RecruitingNote}
 *
 * @property {Object} workPreference
 *   Optional.
 *
 *   The work preference of this candidate.
 *
 *   This object should have the same structure as [WorkPreference]{@link google.cloud.talent.v4beta1.WorkPreference}
 *
 * @property {Object[]} industryExperiences
 *   Optional.
 *
 *   Which industries does this person have experience working in.
 *
 *   This object should have the same structure as [Experience]{@link google.cloud.talent.v4beta1.Experience}
 *
 * @property {Object[]} workEnvironmentExperiences
 *   Optional.
 *
 *   Which work environment(s), such as in a warehouse or a factory, does this
 *   person have experience working in.
 *
 *   This object should have the same structure as [Experience]{@link google.cloud.talent.v4beta1.Experience}
 *
 * @property {Object[]} securityClearances
 *   Optional.
 *
 *   The candidate's list of issued security clearances.
 *
 *   This object should have the same structure as [SecurityClearance]{@link google.cloud.talent.v4beta1.SecurityClearance}
 *
 * @property {Object.<string, Object>} customAttributes
 *   Optional.
 *
 *   A map of fields to hold both filterable and non-filterable custom profile
 *   attributes that aren't covered by the provided structured fields. See
 *   CustomAttribute for more details.
 *
 *   At most 100 filterable and at most 100 unfilterable keys are supported. If
 *   limit is exceeded, an error is thrown. Custom attributes are `unfilterable`
 *   by default. These are filterable when the `filterable` flag is set to
 *   `true`.
 *
 *   Numeric custom attributes: each key can only map to one numeric value,
 *   otherwise an error is thrown. Client can also filter on numeric custom
 *   attributes using '>', '<' or '=' operators.
 *
 *   String custom attributes: each key can map up to 50 string values. For
 *   filterable string value, each value has a byte size of no more than 256B.
 *   For unfilterable string values, the maximum byte size of a single key is
 *   64B. An error is thrown for any request exceeding the limit.
 *   The maximum total byte size is 10KB.
 *
 * @property {string[]} tags
 *   Output only. The tags assigned to this profile. This supports the ability
 *   to filter on tags assigned to the profile.
 *
 *   Tags are set by the AddProfileTags and RemoveProfileTags APIs.
 *
 * @property {boolean} processed
 *   Output only. Indicates if the profile is fully processed and searchable.
 *
 * @property {string} keywordSnippet
 *   Output only. Keyword snippet shows how the search result is related to a
 *   search query.
 *
 * @typedef Profile
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Profile definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Profile = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents the name of a person.
 *
 * @property {string} formattedName
 *   Optional.
 *
 *   A string represents a person's full name. For example, "Dr. John Smith".
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} structuredName
 *   Optional.
 *
 *   A person's name in a structured way (last name, first name, suffix, and
 *   so on.)
 *
 *   This object should have the same structure as [PersonStructuredName]{@link google.cloud.talent.v4beta1.PersonStructuredName}
 *
 * @property {string} preferredName
 *   Optional.
 *
 *   Preferred name for the person.
 *
 * @typedef PersonName
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.PersonName definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const PersonName = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Resource that represents a person's structured name.
   *
   * @property {string} givenName
   *   Optional.
   *
   *   Given/first name.
   *
   *   It's derived from formatted_name if not provided.
   *
   *   Number of characters allowed is 100.
   *
   * @property {string} middleInitial
   *   Optional.
   *
   *   Middle initial.
   *
   *   It's derived from formatted_name if not provided.
   *
   *   Number of characters allowed is 20.
   *
   * @property {string} familyName
   *   Optional.
   *
   *   Family/last name.
   *
   *   It's derived from formatted_name if not provided.
   *
   *   Number of characters allowed is 100.
   *
   * @property {string[]} suffixes
   *   Optional.
   *
   *   Suffixes.
   *
   *   Number of characters allowed is 20.
   *
   * @property {string[]} prefixes
   *   Optional.
   *
   *   Prefixes.
   *
   *   Number of characters allowed is 20.
   *
   * @typedef PersonStructuredName
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.PersonName.PersonStructuredName definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
   */
  PersonStructuredName: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  }
};

/**
 * Resource that represents a address.
 *
 * @property {number} usage
 *   Optional.
 *
 *   The usage of the address. For example, SCHOOL, WORK, PERSONAL.
 *
 *   The number should be among the values of [ContactInfoUsage]{@link google.cloud.talent.v4beta1.ContactInfoUsage}
 *
 * @property {string} unstructuredAddress
 *   Optional.
 *
 *   Unstructured address.
 *
 *   For example, "1600 Amphitheatre Pkwy, Mountain View, CA 94043",
 *   "Sunnyvale, California".
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} structuredAddress
 *   Optional.
 *
 *   Structured address that contains street address, city, state, country,
 *   and so on.
 *
 *   This object should have the same structure as [PostalAddress]{@link google.type.PostalAddress}
 *
 * @property {Object} current
 *   Optional.
 *
 *   Indicates if it's the person's current address.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @typedef Address
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Address definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Address = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a person's email address.
 *
 * @property {number} usage
 *   Optional.
 *
 *   The usage of the email address. For example, SCHOOL, WORK, PERSONAL.
 *
 *   The number should be among the values of [ContactInfoUsage]{@link google.cloud.talent.v4beta1.ContactInfoUsage}
 *
 * @property {string} emailAddress
 *   Optional.
 *
 *   Email address.
 *
 *   Number of characters allowed is 4,000.
 *
 * @typedef Email
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Email definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Email = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a person's telephone number.
 *
 * @property {number} usage
 *   Optional.
 *
 *   The usage of the phone. For example, SCHOOL, WORK, PERSONAL.
 *
 *   The number should be among the values of [ContactInfoUsage]{@link google.cloud.talent.v4beta1.ContactInfoUsage}
 *
 * @property {number} type
 *   Optional.
 *
 *   The phone type. For example, LANDLINE, MOBILE, FAX.
 *
 *   The number should be among the values of [PhoneType]{@link google.cloud.talent.v4beta1.PhoneType}
 *
 * @property {string} number
 *   Optional.
 *
 *   Phone number.
 *
 *   Any phone formats are supported and only exact matches are performed on
 *   searches. For example, if a phone number in profile is provided in the
 *   format of "(xxx)xxx-xxxx", in profile searches the same phone format
 *   has to be provided.
 *
 *   Number of characters allowed is 20.
 *
 * @property {string} whenAvailable
 *   Optional.
 *
 *   When this number is available. Any descriptive string is expected.
 *
 *   Number of characters allowed is 100.
 *
 * @typedef Phone
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Phone definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Phone = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Enum that represents the type of the telephone.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  PhoneType: {

    /**
     * Default value.
     */
    PHONE_TYPE_UNSPECIFIED: 0,

    /**
     * A landline.
     */
    LANDLINE: 1,

    /**
     * A mobile.
     */
    MOBILE: 2,

    /**
     * A fax.
     */
    FAX: 3,

    /**
     * A pager.
     */
    PAGER: 4,

    /**
     * A TTY (test telephone) or TDD (telecommunication device for the deaf).
     */
    TTY_OR_TDD: 5,

    /**
     * A voicemail.
     */
    VOICEMAIL: 6,

    /**
     * A virtual telephone number is a number that can be routed to another
     * number and managed by the user via Web, SMS, IVR, and so on.  It is
     * associated with a particular person, and may be routed to either a MOBILE
     * or LANDLINE number. The phone usage (see ContactInfoUsage above) should
     * be set to PERSONAL for these phone types. Some more information can be
     * found here: http://en.wikipedia.org/wiki/Personal_Numbers
     */
    VIRTUAL: 7,

    /**
     * Voice over IP numbers. This includes TSoIP (Telephony Service over IP).
     */
    VOIP: 8,

    /**
     * In some regions (e.g. the USA), it is impossible to distinguish between
     * fixed-line and mobile numbers by looking at the phone number itself.
     */
    MOBILE_OR_LANDLINE: 9
  }
};

/**
 * Resource that represents a valid URI for a personal use.
 *
 * @property {string} uri
 *   Optional.
 *
 *   The personal URI.
 *
 *   Number of characters allowed is 4,000.
 *
 * @typedef PersonalUri
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.PersonalUri definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const PersonalUri = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents contact information other than phone, email,
 * URI and addresses.
 *
 * @property {number} usage
 *   Optional.
 *
 *   The usage of this contact method. For example, SCHOOL, WORK, PERSONAL.
 *
 *   The number should be among the values of [ContactInfoUsage]{@link google.cloud.talent.v4beta1.ContactInfoUsage}
 *
 * @property {string} name
 *   Optional.
 *
 *   The name of the contact method.
 *
 *   For example, "hangout", "skype".
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} contactId
 *   Optional.
 *
 *   The contact id.
 *
 *   Number of characters allowed is 100.
 *
 * @typedef AdditionalContactInfo
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.AdditionalContactInfo definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const AdditionalContactInfo = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents an employment record of a candidate.
 *
 * @property {Object} startDate
 *   Optional.
 *
 *   Start date of the employment.
 *
 *   It can be a partial date (only year, or only year and month), but must be
 *   valid. Otherwise an error is thrown.
 *
 *   Examples:
 *   {"year": 2017, "month": 2, "day": 28} is valid.
 *   {"year": 2020, "month": 1, "date": 31} is valid.
 *   {"year": 2018, "month": 12} is valid (partial date).
 *   {"year": 2018} is valid (partial date).
 *   {"year": 2015, "day": 21} is not valid (month is missing but day is
 *   presented).
 *   {"year": 2018, "month": 13} is not valid (invalid month).
 *   {"year": 2017, "month": 1, "day": 32} is not valid (invalid day).
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} endDate
 *   Optional.
 *
 *   End date of the employment.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} employerName
 *   Optional.
 *
 *   The name of the employer company/organization.
 *
 *   For example, "Google", "Alphabet", and so on.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} divisionName
 *   Optional.
 *
 *   The division name of the employment.
 *
 *   For example, division, department, client, and so on.
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} address
 *   Optional.
 *
 *   The physical address of the employer.
 *
 *   This object should have the same structure as [Address]{@link google.cloud.talent.v4beta1.Address}
 *
 * @property {string} jobTitle
 *   Optional.
 *
 *   The job title of the employment.
 *
 *   For example, "Software Engineer", "Data Scientist", and so on.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} jobDescription
 *   Optional.
 *
 *   The description of job content.
 *
 *   Number of characters allowed is 100,000.
 *
 * @property {Object} isSupervisor
 *   Optional.
 *
 *   If the jobs is a supervisor position.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object} isSelfEmployed
 *   Optional.
 *
 *   If this employment is self-employed.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object} isCurrent
 *   Optional.
 *
 *   If this employment is current.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {string} jobTitleSnippet
 *   Output only. The job title snippet shows how the job_title is related
 *   to a search query. It's empty if the job_title isn't related to the
 *   search query.
 *
 * @property {string} jobDescriptionSnippet
 *   Output only. The job description snippet shows how the job_description
 *   is related to a search query. It's empty if the job_description isn't
 *   related to the search query.
 *
 * @property {string} employerNameSnippet
 *   Output only. The employer name snippet shows how the employer_name is
 *   related to a search query. It's empty if the employer_name isn't
 *   related to the search query.
 *
 * @typedef EmploymentRecord
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.EmploymentRecord definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const EmploymentRecord = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents an education record of a candidate.
 *
 * @property {Object} startDate
 *   Optional.
 *
 *   The start date of the education.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} endDate
 *   Optional.
 *
 *   The end date of the education.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} expectedGraduationDate
 *   Optional.
 *
 *   The expected graduation date if currently pursuing a degree.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} schoolName
 *   Optional.
 *
 *   The name of the school or institution.
 *
 *   For example, "Stanford University", "UC Berkeley", and so on.
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} address
 *   Optional.
 *
 *   The physical address of the education institution.
 *
 *   This object should have the same structure as [Address]{@link google.cloud.talent.v4beta1.Address}
 *
 * @property {string} degreeDescription
 *   Optional.
 *
 *   The full description of the degree.
 *
 *   For example, "Master of Science in Computer Science", "B.S in Math".
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} structuredDegree
 *   Optional.
 *
 *   The structured notation of the degree.
 *
 *   This object should have the same structure as [Degree]{@link google.cloud.talent.v4beta1.Degree}
 *
 * @property {string} description
 *   Optional.
 *
 *   The description of the education.
 *
 *   Number of characters allowed is 100,000.
 *
 * @property {Object} isCurrent
 *   Optional.
 *
 *   If this education is current.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {string} schoolNameSnippet
 *   Output only. The school name snippet shows how the school_name is
 *   related to a search query in search result. It's empty if the
 *   school_name isn't related to the search query.
 *
 * @property {string} degreeSnippet
 *   Output only. The job description snippet shows how the degree is
 *   related to a search query in search result. It's empty if the degree
 *   isn't related to the search query.
 *
 * @typedef EducationRecord
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.EducationRecord definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const EducationRecord = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a degree pursuing or acquired by a candidate.
 *
 * @property {number} degreeType
 *   Optional.
 *
 *   ISCED degree type.
 *
 *   The number should be among the values of [DegreeType]{@link google.cloud.talent.v4beta1.DegreeType}
 *
 * @property {string} degreeName
 *   Optional.
 *
 *   Full Degree name.
 *
 *   For example, "B.S.", "Master of Arts", and so on.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string[]} fieldsOfStudy
 *   Optional.
 *
 *   Fields of study for the degree.
 *
 *   For example, "Computer science", "engineering".
 *
 *   Number of characters allowed is 100.
 *
 * @typedef Degree
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Degree definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Degree = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents an individual or collaborative activity participated
 * in by a candidate, for example, an open-source project, a class assignment,
 * and so on.
 *
 * @property {string} displayName
 *   Optional.
 *
 *   Activity display name.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} description
 *   Optional.
 *
 *   Activity description.
 *
 *   Number of characters allowed is 100,000.
 *
 * @property {string} uri
 *   Optional.
 *
 *   Activity URI.
 *
 *   Number of characters allowed is 4,000.
 *
 * @property {Object} createDate
 *   Optional.
 *
 *   The first creation date of the activity.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} updateDate
 *   Optional.
 *
 *   The last update date of the activity.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string[]} teamMembers
 *   Optional.
 *
 *   A list of team members involved in this activity.
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object[]} skillsUsed
 *   Optional.
 *
 *   A list of skills used in this activity.
 *
 *   This object should have the same structure as [Skill]{@link google.cloud.talent.v4beta1.Skill}
 *
 * @property {string} activityNameSnippet
 *   Output only. Activity name snippet shows how the display_name is
 *   related to a search query. It's empty if the display_name isn't related
 *   to the search query.
 *
 * @property {string} activityDescriptionSnippet
 *   Output only. Activity description snippet shows how the
 *   description is related to a search query. It's empty if the
 *   description isn't related to the search query.
 *
 * @property {string[]} skillsUsedSnippet
 *   Output only. Skill used snippet shows how the corresponding
 *   skills_used are related to a search query. It's empty if the
 *   corresponding skills_used are not related to the search query.
 *
 * @typedef Activity
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Activity definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Activity = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a publication resource of a candidate.
 *
 * @property {string[]} authors
 *   Optional.
 *
 *   A list of author names.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} title
 *   Optional.
 *
 *   The title of the publication.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} description
 *   Optional.
 *
 *   The description of the publication.
 *
 *   Number of characters allowed is 100,000.
 *
 * @property {string} journal
 *   Optional.
 *
 *   The journal name of the publication.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} volume
 *   Optional.
 *
 *   Volume number.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} publisher
 *   Optional.
 *
 *   The publisher of the journal.
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} publicationDate
 *   Optional.
 *
 *   The publication date.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} publicationType
 *   Optional.
 *
 *   The publication type.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} isbn
 *   Optional.
 *
 *   ISBN number.
 *
 *   Number of characters allowed is 100.
 *
 * @typedef Publication
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Publication definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Publication = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents the patent acquired by a candidate.
 *
 * @property {string} displayName
 *   Optional.
 *
 *   Name of the patent.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string[]} inventors
 *   Optional.
 *
 *   A list of inventors' names.
 *
 *   Number of characters allowed for each is 100.
 *
 * @property {string} patentStatus
 *   Optional.
 *
 *   The status of the patent.
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} patentStatusDate
 *   Optional.
 *
 *   The date the last time the status of the patent was checked.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} patentFilingDate
 *   Optional.
 *
 *   The date that the patent was filed.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} patentOffice
 *   Optional.
 *
 *   The name of the patent office.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} patentNumber
 *   Optional.
 *
 *   The number of the patent.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} patentDescription
 *   Optional.
 *
 *   The description of the patent.
 *
 *   Number of characters allowed is 100,000.
 *
 * @property {Object[]} skillsUsed
 *   Optional.
 *
 *   The skills used in this patent.
 *
 *   This object should have the same structure as [Skill]{@link google.cloud.talent.v4beta1.Skill}
 *
 * @typedef Patent
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Patent definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const Patent = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a work preference.
 *
 * @property {string[]} jobTitles
 *   A list of preferred job titles.
 *
 * @property {string} jobObjective
 *   A paragraph displaying the persons' job objectives.
 *
 * @property {Object[]} jobAddresses
 *   A list of preferred job addresses.
 *
 *   This object should have the same structure as [Address]{@link google.cloud.talent.v4beta1.Address}
 *
 * @property {string[]} jobChangeReasons
 *   A person's reason for changing jobs.
 *
 * @property {string[]} otherPreferences
 *   A list of preferred job attributes / responsibilities / qualifications /
 *   companies / and so on. as determined by the candidate.
 *   Effectively the candidate's answer to the question
 *   "What is most important to you when considering a new job?".
 *
 * @property {number[]} employmentTypes
 *   A list of preferred employment types.
 *
 *   The number should be among the values of [EmploymentType]{@link google.cloud.talent.v4beta1.EmploymentType}
 *
 * @property {string[]} employeeTypes
 *   A list of preferred employee types (e.g. W2, Vendor, Unknown) which can
 *   be orthogonal to EmploymentType preferences.
 *
 * @property {Object[]} preferredCompensation
 *   Preferred compensation. This is repeated if the candidate expressed
 *   compensation preferences differently over time or for different jobs.
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {Object} minCompensation
 *   Minimum acceptable compensation.
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {Object} preferredCommute
 *   Preferred commute time candidate is willing to
 *   accept to the work location.
 *
 *   This object should have the same structure as [Duration]{@link google.protobuf.Duration}
 *
 * @property {Object} maxCommute
 *   Maximum commute time candidate is willing to accept to the work location.
 *
 *   This object should have the same structure as [Duration]{@link google.protobuf.Duration}
 *
 * @property {Object} preferredCommuteDistanceMiles
 *   Preferred distance candidate is willing to accept to the work location.
 *
 *   This object should have the same structure as [DoubleValue]{@link google.protobuf.DoubleValue}
 *
 * @property {Object} maxCommuteDistanceMiles
 *   Maximum distance candidate is willing to accept to the work location.
 *
 *   This object should have the same structure as [DoubleValue]{@link google.protobuf.DoubleValue}
 *
 * @property {number[]} commuteMethods
 *   The preferred commute methods.
 *
 *   The number should be among the values of [CommuteMethod]{@link google.cloud.talent.v4beta1.CommuteMethod}
 *
 * @property {Object} canRelocate
 *   The candidate is willing to relocate for work.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object} hasReliableTransportation
 *   If true, the candidate has a reliable independent mode of transportation.
 *   For example, automobile.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object} requiresPublicTransportation
 *   If true, the candidate requires job locations with readily accessible
 *   modes of public transportation.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object[]} jobRequirementInterests
 *   Optional.
 *
 *   The requirements making a person interested (or not) in a job.
 *
 *   This object should have the same structure as [JobRequirementInterest]{@link google.cloud.talent.v4beta1.JobRequirementInterest}
 *
 * @typedef WorkPreference
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.WorkPreference definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const WorkPreference = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * The requirements making a person interested (or not) in a job.
 *
 * @property {Object} jobRequirement
 *   The requirement of a job.
 *
 *   This object should have the same structure as [JobRequirement]{@link google.cloud.talent.v4beta1.JobRequirement}
 *
 * @property {boolean} isInterested
 *   If true, the candidate is interested in jobs with this requirement.
 *
 * @typedef JobRequirementInterest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.JobRequirementInterest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const JobRequirementInterest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a security clearance.
 *
 * @property {string} level
 *   The level or type of the security clearance.
 *
 * @property {Object} expireDate
 *   The expiration date of the security clearance issued.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} active
 *   This is an active security clearance.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {Object} isActiveTwoYears
 *   This is a clearance which was active in the last two years.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @typedef SecurityClearance
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.SecurityClearance definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const SecurityClearance = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a profile update history.
 *
 * @property {Object} updateTime
 *   Required.
 *
 *   When the profile information is updated.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} updateReasonNote
 *   The reason this update occurred.
 *
 * @typedef FieldUpdateMetadata
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.FieldUpdateMetadata definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/profile.proto}
 */
const FieldUpdateMetadata = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};