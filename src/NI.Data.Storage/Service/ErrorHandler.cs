#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
 * Distributed under the LGPL licence
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.Net;

namespace NI.Data.Storage.Service {

	public class ErrorHandler : IErrorHandler {

		static Logger log = new Logger(typeof(ErrorHandler));

		public ErrorHandler() {

		}

		public bool HandleError(Exception error) {
			log.Error("Storage API error: {0}", error);
			return true;
		}

		public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault) {
			var faultEx = new FaultException<ApiFault>( new ApiFault(error.Message), new FaultReason(error.Message) );
			fault = Message.CreateMessage(version, faultEx.CreateMessageFault(), "error" );

			fault.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Xml));

			WebOperationContext.Current.OutgoingResponse.StatusDescription = error.Message;
			//WebOperationContext.Current.OutgoingResponse.SuppressEntityBody = true;

			// specal hack to prevent forms auth redirection
			var code = error is ApiException ? ((ApiException)error).StatusCode : System.Net.HttpStatusCode.InternalServerError;
			if (error is System.Runtime.Serialization.SerializationException)
				code = HttpStatusCode.BadRequest;

			WebOperationContext.Current.OutgoingResponse.StatusCode = code == HttpStatusCode.Unauthorized ? HttpStatusCode.Unauthorized : code;
			if (error is ApiException) {
				WebOperationContext.Current.OutgoingResponse.Headers["error_id"] = ((ApiException)error).ErrorId;
			}

		}

	}

	// This attribute can be used to install a custom error handler for a service.
	public class ErrorBehaviorAttribute : Attribute, IServiceBehavior {
		Type errorHandlerType;

		public ErrorBehaviorAttribute(Type errorHandlerType) {
			this.errorHandlerType = errorHandlerType;
		}

		void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase) {
		}

		void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters) {
		}

		void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase) {
			IErrorHandler errorHandler;

			try {
				errorHandler = (IErrorHandler)Activator.CreateInstance(errorHandlerType);
			} catch (MissingMethodException e) {
				throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must have a public empty constructor.", e);
			} catch (InvalidCastException e) {
				throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must implement System.ServiceModel.Dispatcher.IErrorHandler.", e);
			}

			foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers) {
				ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
				channelDispatcher.ErrorHandlers.Clear();
				channelDispatcher.ErrorHandlers.Add(errorHandler);
			}
		}
	}


}
