using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading;

namespace MyAgencyVault.BusinessLibrary.PostProcess
{
    public class DeuPostProcessWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_PostEntryProcess"></param>
        /// <param name="deuFields"></param>
        /// <param name="deuEntryId">
        /// FirstPost = Guid.Empty
        /// RePort = OldDeuEntryId
        /// DeletePost = DeuEntryId
        /// </param>
        /// <param name="userRole"></param>
        /// <returns></returns>
        public static PostProcessReturnStatus DeuPostStartWrapper(PostEntryProcess _PostEntryProcess, DEUFields deuFields, Guid deuEntryId, Guid userId, UserRole userRole)
        {
            //bool lockObtained = false;
            DEUFields tempDeuFields = null;
            BasicInformationForProcess _BasicInformationForProcess = null;

            PostProcessReturnStatus _PostProcessReturnStatus = null;

            if (_PostEntryProcess == PostEntryProcess.FirstPost || _PostEntryProcess == PostEntryProcess.RePost)
            {

                DEU objDeu = new DEU();
                ModifiyableBatchStatementData batchStatementData = objDeu.AddUpdate(deuFields, deuEntryId);


                if (batchStatementData == null)
                    return _PostProcessReturnStatus;

                if (batchStatementData.ExposedDeu == null)
                    return _PostProcessReturnStatus;

                if (batchStatementData.ExposedDeu.DEUENtryID == null)
                    return _PostProcessReturnStatus;

                deuFields.DeuEntryId = batchStatementData.ExposedDeu.DEUENtryID;

                if (deuFields != null)
                {
                    if (deuFields.DeuEntryId != null)
                    {
                        tempDeuFields = PostUtill.FillDEUFields(deuFields.DeuEntryId);
                    }
                }

                if (deuEntryId != Guid.Empty)
                {
                    tempDeuFields = PostUtill.FillDEUFields(deuEntryId);
                    _BasicInformationForProcess = PostUtill.GetPolicyToProcess(tempDeuFields, string.Empty);
                }

            }
            else
            {
                tempDeuFields = PostUtill.FillDEUFields(deuEntryId);
                _BasicInformationForProcess = PostUtill.GetPolicyToProcess(tempDeuFields, string.Empty);

            }

            _PostProcessReturnStatus = new PostProcessReturnStatus() { DeuEntryId = Guid.Empty, IsComplete = false, ErrorMessage = null, PostEntryStatus = _PostEntryProcess };

            if (_PostEntryProcess == PostEntryProcess.FirstPost)
            {


                _PostProcessReturnStatus = PostUtill.PostStart(_PostEntryProcess, deuFields.DeuEntryId, deuEntryId, userId, userRole, _PostEntryProcess, string.Empty, string.Empty);

                _PostProcessReturnStatus.DeuEntryId = deuFields.DeuEntryId;
                _PostProcessReturnStatus.OldDeuEntryId = Guid.Empty;
                _PostProcessReturnStatus.ReferenceNo = deuFields.ReferenceNo;


            }
            else if (_PostEntryProcess == PostEntryProcess.RePost)
            {

                _PostProcessReturnStatus = PostUtill.PostStart(_PostEntryProcess, deuEntryId, deuFields.DeuEntryId, userId, userRole, _PostEntryProcess, string.Empty, string.Empty);
                _PostProcessReturnStatus.DeuEntryId = deuFields.DeuEntryId;
                _PostProcessReturnStatus.OldDeuEntryId = deuEntryId;
                _PostProcessReturnStatus.ReferenceNo = deuFields.ReferenceNo;

            }
            else
            {

                _PostProcessReturnStatus = PostUtill.PostStart(_PostEntryProcess, deuEntryId, Guid.Empty, userId, userRole, _PostEntryProcess, string.Empty, string.Empty);
                _PostProcessReturnStatus.DeuEntryId = deuEntryId;
                _PostProcessReturnStatus.OldDeuEntryId = Guid.Empty;

            }
            return _PostProcessReturnStatus;
        }
    }
}
