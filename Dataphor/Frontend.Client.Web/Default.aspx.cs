/*
	Alphora Dataphor
	© Copyright 2000-2008 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/

using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Net;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Alphora.Dataphor.BOP;
using Alphora.Dataphor.Frontend.Client;

namespace Alphora.Dataphor.Frontend.Client.Web
{
	// Do not put classes above WebClient

	/// <summary> WebClient is what the web client page inherits from. </summary>
	public class Default : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			FWebSession = (Web.Session)Session["WebSession"];
			if (FWebSession == null)
				Response.Redirect((string)Session["ConnectPage"]);

			Response.Buffer = true;
			Response.CacheControl = "no-cache";

			FWebSession.ProcessRequest(Context);

			if (FWebSession.Forms.IsEmpty())
			{
				FWebSession.Dispose();
				FWebSession = null;
				Session.Remove("WebSession");
				string LDestination = (string)Session["CompletedPage"];
				if ((LDestination == null) || (LDestination == String.Empty))
					LDestination = (string)Session["ConnectPage"];
				Response.Redirect(LDestination);
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		// WebSession

		private Web.Session FWebSession;
		public Web.Session WebSession { get { return FWebSession; } }

		public void WriteBodyAttributes()
		{
			if (!WebSession.Forms.IsEmpty())
			{
				string LImageID = ((IWebFormInterface)WebSession.Forms.GetTopmostForm()).BackgroundImageID;
				if (LImageID != String.Empty)
					Response.Write(@" background='ViewImage.aspx?ImageID=" + LImageID + "'");
			}

			// handle repositioning of page to same scroll position after submit
			Response.Write(@" onscroll=""document.getElementById('ScrollPosition').value = MainBody.scrollTop;""");
			string LPosition = Request.Form["ScrollPosition"];
			if ((LPosition != null) && (LPosition != String.Empty))
				Response.Write(String.Format(@" onload=""MainBody.scrollTop={0};""", Convert.ToInt32(LPosition)));
		}
	}
}