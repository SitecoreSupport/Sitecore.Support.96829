namespace Sitecore.Support.Shell.Applications.Layouts.DeviceEditor
{
    using Sitecore.Shell.Applications.Layouts.DeviceEditor;
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web.UI.HtmlControls;
    using System.Xml.Linq;

    

    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Layouts;
    using Sitecore.Pipelines.RenderDeviceEditorRendering;
    using Sitecore.Resources;
    using Sitecore.Rules;
    using Sitecore.SecurityModel;
    using Sitecore.Shell.Applications.Dialogs;
    using Sitecore.Shell.Applications.Dialogs.ItemLister;
    using Sitecore.Shell.Applications.Dialogs.Personalize;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.PageModes;
    using Sitecore.Web.UI.Pages;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.XmlControls;
    using Sitecore.Shell.Applications.Dialogs.Testing;

    /// <summary>
    /// Represents the Device Editor form.
    /// </summary>
    [UsedImplicitly]
    public class DeviceEditorForm : DialogForm
    {
        /// <summary>
        ///   The command name.
        /// </summary>
        private const string CommandName = "device:settestdetails";

        #region Properties

        /// <summary>
        /// Gets or sets the controls.
        /// </summary>
        /// <value>The controls.</value>
        [NotNull]
        public ArrayList Controls
        {
            get
            {
                return (ArrayList)Context.ClientPage.ServerProperties["Controls"];
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");

                Context.ClientPage.ServerProperties["Controls"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        /// <value>The device ID.</value>
        [NotNull]
        public string DeviceID
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["DeviceID"]);
            }

            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");

                Context.ClientPage.ServerProperties["DeviceID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected.
        /// </summary>
        /// <value>The index of the selected.</value>
        public int SelectedIndex
        {
            get
            {
                return MainUtil.GetInt(Context.ClientPage.ServerProperties["SelectedIndex"], -1);
            }

            set
            {
                Context.ClientPage.ServerProperties["SelectedIndex"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        /// <value>The unique id.</value>
        [NotNull]
        public string UniqueId
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["PlaceholderUniqueID"]);
            }

            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");

                Context.ClientPage.ServerProperties["PlaceholderUniqueID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>The layout.</value>
        protected TreePicker Layout { get; set; }

        /// <summary>
        /// Gets or sets the placeholders.
        /// </summary>
        /// <value>The placeholders.</value>
        protected Scrollbox Placeholders { get; set; }

        /// <summary>
        /// Gets or sets the renderings.
        /// </summary>
        /// <value>The renderings.</value>
        protected Scrollbox Renderings { get; set; }

        /// <summary>
        /// Gets or sets the test.
        /// </summary>
        /// <value>The test button.</value>
        protected Button Test { get; set; }

        /// <summary>
        /// Gets or sets the personalize button control.
        /// </summary>
        /// <value>The personalize button control.</value>
        protected Button Personalize { get; set; }

        /// <summary>
        /// Gets or sets the edit.
        /// </summary>
        /// <value>The edit button.</value>
        protected Button btnEdit { get; set; }

        /// <summary>
        /// Gets or sets the change.
        /// </summary>
        /// <value>The change button.</value>
        protected Button btnChange { get; set; }

        /// <summary>
        /// Gets or sets the remove.
        /// </summary>
        /// <value>The Remove button.</value>
        protected Button btnRemove { get; set; }

        /// <summary>
        /// Gets or sets the move up.
        /// </summary>
        /// <value>The Move Up button.</value>
        protected Button MoveUp { get; set; }

        /// <summary>
        /// Gets or sets the move down.
        /// </summary>
        /// <value>The Move Down button.</value>
        protected Button MoveDown { get; set; }

        /// <summary>
        /// Gets or sets the Edit placeholder button.
        /// </summary>
        /// <value>The Edit placeholder button.</value>
        protected Button phEdit { get; set; }

        /// <summary>
        /// Gets or sets the phRemove button.
        /// </summary>
        /// <value>Remove place holder button.</value>
        protected Button phRemove { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:add", true)]
        [UsedImplicitly]
        protected void Add([NotNull] ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    string[] parts = args.Result.Split(',');

                    string itemId = parts[0];
                    string placeholderName = parts[1].Replace("-c-", ",");
                    bool openProperties = parts[2] == "1";

                    LayoutDefinition layout = GetLayoutDefinition();
                    DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

                    var renderingDefinition = new RenderingDefinition
                    {
                        ItemID = itemId,
                        Placeholder = placeholderName
                    };

                    deviceDefinition.AddRendering(renderingDefinition);

                    SetDefinition(layout);

                    this.Refresh();

                    if (openProperties)
                    {
                        ArrayList renderings = deviceDefinition.Renderings;
                        if (renderings != null)
                        {
                            this.SelectedIndex = renderings.Count - 1;
                            Context.ClientPage.SendMessage(this, "device:edit");
                        }
                    }

                    Registry.SetString("/Current_User/SelectRendering/Selected", itemId);
                }
            }
            else
            {
                var options = new SelectRenderingOptions
                {
                    ShowOpenProperties = true,
                    ShowPlaceholderName = true,
                    PlaceholderName = string.Empty
                };

                string itemId = Registry.GetString("/Current_User/SelectRendering/Selected");
                if (!string.IsNullOrEmpty(itemId))
                {
                    options.SelectedItem = Client.ContentDatabase.GetItem(itemId);
                }

                string url = options.ToUrlString(Client.ContentDatabase).ToString();

                SheerResponse.ShowModalDialog(url, true);

                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Adds the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:addplaceholder", true)]
        [UsedImplicitly]
        protected void AddPlaceholder([NotNull] ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
                {
                    LayoutDefinition layout = GetLayoutDefinition();
                    DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);
                    string placeholderKey;
                    var item = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out placeholderKey);
                    if (item == null || string.IsNullOrEmpty(placeholderKey))
                    {
                        return;
                    }

                    var placeholderDefinition = new PlaceholderDefinition
                    {
                        UniqueId = ID.NewID.ToString(),
                        MetaDataItemId = item.Paths.FullPath,
                        Key = placeholderKey
                    };

                    deviceDefinition.AddPlaceholder(placeholderDefinition);
                    SetDefinition(layout);
                    this.Refresh();
                }
            }
            else
            {
                var options = new SelectPlaceholderSettingsOptions
                {
                    IsPlaceholderKeyEditable = true
                };

                SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), "460px", "460px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Adds the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:change", true)]
        [UsedImplicitly]
        protected void Change([NotNull] ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (this.SelectedIndex < 0)
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }

            var renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(renderingDefinition.ItemID))
            {
                return;
            }

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    string[] parts = args.Result.Split(',');

                    renderingDefinition.ItemID = parts[0];
                    bool openProperties = parts[2] == "1";

                    SetDefinition(layout);

                    this.Refresh();

                    if (openProperties)
                    {
                        Context.ClientPage.SendMessage(this, "device:edit");
                    }
                }
            }
            else
            {
                var options = new SelectRenderingOptions
                {
                    ShowOpenProperties = true,
                    ShowPlaceholderName = false,
                    PlaceholderName = string.Empty,
                    SelectedItem = Client.ContentDatabase.GetItem(renderingDefinition.ItemID)
                };

                string url = options.ToUrlString(Client.ContentDatabase).ToString();

                SheerResponse.ShowModalDialog(url, true);

                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Edits the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:edit", true)]
        [UsedImplicitly]
        protected void Edit([NotNull] ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            var options = new RenderingParameters
            {
                Args = args,
                DeviceId = this.DeviceID,
                SelectedIndex = this.SelectedIndex,
                Item = UIUtil.GetItemFromQueryString(Client.ContentDatabase)
            };

            if (options.Show())
            {
                this.Refresh();
            }

            /*
            if(args.IsPostBack){
              Refresh();
            }
            else{
              if(SelectedIndex >= 0){
                UrlString url = new UrlString(UIUtil.GetUri("control:RenderingEditor"));

                url.Append("de", DeviceID);
                url.Append("ix", SelectedIndex.ToString());

                SheerResponse.ShowModalDialog(url.ToString(), true);

                args.WaitForPostBack();
              }
            }
            */
        }

        /// <summary>
        /// Edits the placeholder.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:editplaceholder", true)]
        [UsedImplicitly]
        protected void EditPlaceholder([NotNull] ClientPipelineArgs args)
        {
            if (string.IsNullOrEmpty(this.UniqueId))
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();
            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);
            var placeholderDefinition = deviceDefinition.GetPlaceholder(this.UniqueId);
            if (placeholderDefinition == null)
            {
                return;
            }

            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
                {
                    string placeholderKey;
                    var item = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out placeholderKey);
                    if (item == null)
                    {
                        return;
                    }

                    placeholderDefinition.MetaDataItemId = item.Paths.FullPath;
                    placeholderDefinition.Key = placeholderKey;
                    SetDefinition(layout);
                    this.Refresh();
                }
            }
            else
            {
                var settingsItem = string.IsNullOrEmpty(placeholderDefinition.MetaDataItemId) ? null : Client.ContentDatabase.GetItem(placeholderDefinition.MetaDataItemId);
                var options = new SelectPlaceholderSettingsOptions
                {
                    TemplateForCreating = null,
                    PlaceholderKey = placeholderDefinition.Key,
                    CurrentSettingsItem = settingsItem,
                    SelectedItem = settingsItem,
                    IsPlaceholderKeyEditable = true
                };

                SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), "460px", "460px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// The set test
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:test", true), UsedImplicitly]
        protected void SetTest([NotNull] ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (this.SelectedIndex < 0)
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();
            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);
            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }

            var renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    if (args.Result == "#reset#")
                    {
                        renderingDefinition.MultiVariateTest = string.Empty;
                        SetDefinition(layout);
                        this.Refresh();
                        return;
                    }

                    var id = SetTestDetailsOptions.ParseDialogResult(args.Result);
                    if (ID.IsNullOrEmpty(id))
                    {
                        SheerResponse.Alert(Texts.ITEM_NOT_FOUND);
                        return;
                    }

                    renderingDefinition.MultiVariateTest = id.ToString();
                    SetDefinition(layout);
                    this.Refresh();
                }
            }
            else
            {
                Command deviceTestCommand = CommandManager.GetCommand(CommandName);
                Assert.IsNotNull(deviceTestCommand, "deviceTestCommand");

                var context = new CommandContext();
                context.Parameters["deviceDefinitionId"] = deviceDefinition.ID;
                context.Parameters["renderingDefinitionUniqueId"] = renderingDefinition.UniqueId;

                deviceTestCommand.Execute(context);

                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Raises the load event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// This method notifies the server control that it should perform actions common to each HTTP
        /// request for the page it is associated with, such as setting up a database query. At this
        /// stage in the page life cycle, server controls in the hierarchy are created and initialized,
        /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
        /// property to determine whether the page is being loaded in response to a client post back,
        /// or if it is being loaded and accessed for the first time.
        /// </remarks>
        protected override void OnLoad([NotNull] EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");

            base.OnLoad(e);

            if (!Context.ClientPage.IsEvent)
            {
                this.DeviceID = WebUtil.GetQueryString("de");

                LayoutDefinition layout = GetLayoutDefinition();

                DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);
                if (deviceDefinition.Layout != null)
                {
                    this.Layout.Value = deviceDefinition.Layout;
                }

                this.Personalize.Visible = Policy.IsAllowed(Constants.Capabilities.PersonalizationPolicy);

                Command deviceTestCommand = CommandManager.GetCommand(CommandName);
                this.Test.Visible = deviceTestCommand != null && deviceTestCommand.QueryState(CommandContext.Empty) != CommandState.Hidden;

                this.Refresh();

                this.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Handles a click on the OK button.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The arguments.
        /// </param>
        /// <remarks>
        /// When the user clicks OK, the dialog is closed by calling
        /// the <see cref="Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.
        /// </remarks>
        protected override void OnOK([NotNull] object sender, [NotNull] EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");

            if (this.Layout.Value.Length > 0)
            {
                Item item = Client.ContentDatabase.GetItem(this.Layout.Value);

                if (item == null)
                {
                    Context.ClientPage.ClientResponse.Alert(Texts.LAYOUT_NOT_FOUND);
                    return;
                }

                if (item.TemplateID == TemplateIDs.Folder || item.TemplateID == TemplateIDs.Node)
                {
                    Context.ClientPage.ClientResponse.Alert(Translate.Text(Texts._0_IS_NOT_A_LAYOUT, item.GetUIDisplayName()));
                    return;
                }
            }

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings != null && renderings.Count > 0 && this.Layout.Value.Length == 0)
            {
                Context.ClientPage.ClientResponse.Alert(Texts.YOU_MUST_SPECIFY_A_LAYOUT_WHEN_SPECIFING_RENDERINGS);
                return;
            }

            deviceDefinition.Layout = this.Layout.Value;

            SetDefinition(layout);

            Context.ClientPage.ClientResponse.SetDialogValue("yes");

            base.OnOK(sender, args);
        }

        /// <summary>
        /// Called when the rendering has click.
        /// </summary>
        /// <param name="uniqueId">
        /// The unique Id.
        /// </param>
        [UsedImplicitly]
        protected void OnPlaceholderClick([NotNull] string uniqueId)
        {
            Assert.ArgumentNotNullOrEmpty(uniqueId, "uniqueId");

            if (!string.IsNullOrEmpty(this.UniqueId))
            {
                SheerResponse.SetStyle("ph_" + ID.Parse(this.UniqueId).ToShortID(), "background", string.Empty);
            }

            this.UniqueId = uniqueId;

            if (!string.IsNullOrEmpty(uniqueId))
            {
                SheerResponse.SetStyle("ph_" + ID.Parse(uniqueId).ToShortID(), "background", "#D0EBF6");
            }

            this.UpdatePlaceholdersCommandsState();
        }

        /// <summary>
        /// Called when the rendering has click.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        [UsedImplicitly]
        protected void OnRenderingClick([NotNull] string index)
        {
            Assert.ArgumentNotNull(index, "index");

            if (this.SelectedIndex >= 0)
            {
                SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", string.Empty);
            }

            this.SelectedIndex = MainUtil.GetInt(index, -1);

            if (this.SelectedIndex >= 0)
            {
                SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", "#D0EBF6");
            }

            this.UpdateRenderingsCommandsState();
        }

        /// <summary>
        /// Personalizes the selected control.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:personalize", true)]
        [UsedImplicitly]
        protected void PersonalizeControl([NotNull] ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (this.SelectedIndex < 0)
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }

            var renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(renderingDefinition.ItemID) || string.IsNullOrEmpty(renderingDefinition.UniqueId))
            {
                return;
            }

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    XElement updatedRules = XElement.Parse(args.Result);
                    renderingDefinition.Rules = updatedRules;
                    SetDefinition(layout);

                    this.Refresh();
                }
            }
            else
            {
                var item = UIUtil.GetItemFromQueryString(Client.ContentDatabase);
                string contextItemUri = item != null ? item.Uri.ToString() : string.Empty;
                var options = new PersonalizeOptions
                {
                    SessionHandle = GetSessionHandle(),
                    DeviceId = this.DeviceID,
                    RenderingUniqueId = renderingDefinition.UniqueId,
                    ContextItemUri = contextItemUri
                };

                SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), "980px", "712px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Removes the specified message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [HandleMessage("device:remove")]
        [UsedImplicitly]
        protected void Remove([NotNull] Message message)
        {
            Assert.ArgumentNotNull(message, "message");

            int index = this.SelectedIndex;

            if (index < 0)
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }

            if (index < 0 || index >= renderings.Count)
            {
                return;
            }

            renderings.RemoveAt(index);

            if (index >= 0)
            {
                this.SelectedIndex--;
            }

            SetDefinition(layout);

            this.Refresh();
        }

        /// <summary>
        /// Removes the placeholder.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [HandleMessage("device:removeplaceholder")]
        [UsedImplicitly]
        protected void RemovePlaceholder([NotNull] Message message)
        {
            Assert.ArgumentNotNull(message, "message");

            if (string.IsNullOrEmpty(this.UniqueId))
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            PlaceholderDefinition placeholderDefinition = deviceDefinition.GetPlaceholder(this.UniqueId);
            if (placeholderDefinition == null)
            {
                return;
            }

            ArrayList placeholders = deviceDefinition.Placeholders;
            if (placeholders != null)
            {
                placeholders.Remove(placeholderDefinition);
            }

            SetDefinition(layout);

            this.Refresh();
        }

        /// <summary>
        /// Sorts the down.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [HandleMessage("device:sortdown")]
        [UsedImplicitly]
        protected void SortDown([NotNull] Message message)
        {
            Assert.ArgumentNotNull(message, "message");

            if (this.SelectedIndex < 0)
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();
            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }

            if (this.SelectedIndex >= renderings.Count - 1)
            {
                return;
            }

            var renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }

            renderings.Remove(renderingDefinition);
            renderings.Insert(this.SelectedIndex + 1, renderingDefinition);

            this.SelectedIndex++;

            SetDefinition(layout);

            this.Refresh();
        }

        /// <summary>
        /// Sorts the up.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [HandleMessage("device:sortup")]
        [UsedImplicitly]
        protected void SortUp([NotNull] Message message)
        {
            Assert.ArgumentNotNull(message, "message");

            if (this.SelectedIndex <= 0)
            {
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }

            var renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }

            renderings.Remove(renderingDefinition);
            renderings.Insert(this.SelectedIndex - 1, renderingDefinition);

            this.SelectedIndex--;

            SetDefinition(layout);

            this.Refresh();
        }

        /// <summary>
        /// Gets the layout definition.
        /// </summary>
        /// <returns>
        /// The layout definition.
        /// </returns>
        /// <contract><ensures condition="not null"/></contract>
        [NotNull]
        private static LayoutDefinition GetLayoutDefinition()
        {
            string xml = WebUtil.GetSessionString(GetSessionHandle());
            Assert.IsNotNull(xml, "layout definition");
            return LayoutDefinition.Parse(xml);
        }

        /// <summary>
        /// Gets the session handle.
        /// </summary>
        /// <returns>
        /// The session handle string.
        /// </returns>
        [NotNull]
        private static string GetSessionHandle()
        {
            return "SC_DEVICEEDITOR";
        }

        /// <summary>
        /// Sets the definition.
        /// </summary>
        /// <param name="layout">
        /// The layout.
        /// </param>
        private static void SetDefinition([NotNull] LayoutDefinition layout)
        {
            Assert.ArgumentNotNull(layout, "layout");

            var xml = layout.ToXml();
            WebUtil.SetSessionValue(GetSessionHandle(), xml);
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void Refresh()
        {
            this.Renderings.Controls.Clear();
            this.Placeholders.Controls.Clear();

            this.Controls = new ArrayList();

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            if (deviceDefinition.Renderings == null)
            {
                SheerResponse.SetOuterHtml("Renderings", this.Renderings);
                SheerResponse.SetOuterHtml("Placeholders", this.Placeholders);
                SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
                return;
            }

            int selectedIndex = this.SelectedIndex;

            this.RenderRenderings(deviceDefinition, selectedIndex, 0);
            this.RenderPlaceholders(deviceDefinition);
            this.UpdateRenderingsCommandsState();
            this.UpdatePlaceholdersCommandsState();

            SheerResponse.SetOuterHtml("Renderings", this.Renderings);
            SheerResponse.SetOuterHtml("Placeholders", this.Placeholders);
            SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
        }

        /// <summary>
        /// Renders the placeholders.
        /// </summary>
        /// <param name="deviceDefinition">
        /// The device definition.
        /// </param>
        private void RenderPlaceholders([NotNull] DeviceDefinition deviceDefinition)
        {
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");

            ArrayList placeholders = deviceDefinition.Placeholders;
            if (placeholders == null)
            {
                return;
            }

            foreach (PlaceholderDefinition placeholder in placeholders)
            {
                Item item = null;

                string metaDataItemId = placeholder.MetaDataItemId;
                if (!string.IsNullOrEmpty(metaDataItemId))
                {
                    item = Client.ContentDatabase.GetItem(metaDataItemId);
                }

                var control = Resource.GetWebControl("DeviceRendering") as XmlControl;
                Assert.IsNotNull(control, typeof(XmlControl));

                this.Placeholders.Controls.Add(control);
                ID uid = ID.Parse(placeholder.UniqueId);

                if (placeholder.UniqueId == this.UniqueId)
                {
                    control["Background"] = "#D0EBF6";
                }

                string id = "ph_" + uid.ToShortID();

                control["ID"] = id;
                control["Header"] = placeholder.Key;
                control["Click"] = "OnPlaceholderClick(\"" + placeholder.UniqueId + "\")";
                control["DblClick"] = "device:editplaceholder";

                if (item != null)
                {
                    control["Icon"] = item.Appearance.Icon;
                }
                else
                {
                    control["Icon"] = "Imaging/24x24/layer_blend.png";
                }
            }
        }

        /// <summary>
        /// Renders the specified device definition.
        /// </summary>
        /// <param name="deviceDefinition">
        /// The device definition.
        /// </param>
        /// <param name="selectedIndex">
        /// Index of the selected.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        private void RenderRenderings([NotNull] DeviceDefinition deviceDefinition, int selectedIndex, int index)
        {
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }

            foreach (RenderingDefinition rendering in renderings)
            {
                if (rendering.ItemID == null)
                {
                    continue;
                }

                Item item = Client.ContentDatabase.GetItem(rendering.ItemID);

                var control = Resource.GetWebControl("DeviceRendering") as XmlControl;
                Assert.IsNotNull(control, typeof(XmlControl));

                var container = new HtmlGenericControl("div");

                container.Style.Add("padding", "0");
                container.Style.Add("margin", "0");
                container.Style.Add("border", "0");
                container.Style.Add("position", "relative");
                container.Controls.Add(control);
                string id = Control.GetUniqueID("R");
                this.Renderings.Controls.Add(container);
                container.ID = Control.GetUniqueID("C");

                control["Click"] = "OnRenderingClick(\"" + index + "\")";
                control["DblClick"] = "device:edit";

                if (index == selectedIndex)
                {
                    control["Background"] = "#D0EBF6";
                }

                this.Controls.Add(id);

                if (item != null)
                {
                    control["ID"] = id;
                    control["Icon"] = item.Appearance.Icon;
                    control["Header"] = item.GetUIDisplayName();
                    control["Placeholder"] = WebUtil.SafeEncode((rendering.Placeholder != null) ? WebUtil.SafeEncode(rendering.Placeholder) : string.Empty);
                }
                else
                {
                    control["ID"] = id;
                    control["Icon"] = "Applications/24x24/forbidden.png";
                    control["Header"] = Texts.UNKNOWN_RENDERING;
                    control["Placeholder"] = string.Empty;
                }

                if ((rendering.Rules != null) && !rendering.Rules.IsEmpty)
                {
                    int conditionsCount = rendering.Rules.Elements("rule").Count();
                    if (conditionsCount > 1)
                    {
                        var bage = new HtmlGenericControl("span");
                        if (conditionsCount > 9)
                        {
                            bage.Attributes["class"] = "scConditionContainer scLongConditionContainer";
                        }
                        else
                        {
                            bage.Attributes["class"] = "scConditionContainer";
                        }

                        bage.InnerText = conditionsCount.ToString();
                        container.Controls.Add(bage);
                    }
                }

                RenderDeviceEditorRenderingPipeline.Run(rendering, control, container);

                index++;
            }
        }

        /// <summary>
        /// Updates the state of the commands.
        /// </summary>
        private void UpdateRenderingsCommandsState()
        {
            if (this.SelectedIndex < 0)
            {
                this.ChangeButtonsState(true);
                return;
            }

            LayoutDefinition layout = GetLayoutDefinition();

            DeviceDefinition deviceDefinition = layout.GetDevice(this.DeviceID);

            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                this.ChangeButtonsState(true);
                return;
            }

            var renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                this.ChangeButtonsState(true);
                return;
            }

            this.ChangeButtonsState(false);
            this.Personalize.Disabled = !string.IsNullOrEmpty(renderingDefinition.MultiVariateTest);
            this.Test.Disabled = HasRenderingRules(renderingDefinition);
        }

        private void UpdatePlaceholdersCommandsState()
        {
            this.phEdit.Disabled = String.IsNullOrEmpty(this.UniqueId);
            this.phRemove.Disabled = String.IsNullOrEmpty(this.UniqueId);
        }

        /// <summary>
        /// Changes the disable of the buttons.
        /// </summary>
        /// <param name="disable">if set to <c>true</c> buttons are disabled.</param>
        private void ChangeButtonsState(bool disable)
        {
            this.Personalize.Disabled = disable;
            this.btnEdit.Disabled = disable;
            this.btnChange.Disabled = disable;
            this.btnRemove.Disabled = disable;
            this.MoveUp.Disabled = disable;
            this.MoveDown.Disabled = disable;
            this.Test.Disabled = disable;
        }

        /// <summary>
        /// Determines whether [has rendering rules] [the specified definition].
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns><c>true</c> if the definition has a defined rule with action; otherwise, <c>false</c>.</returns>
        private static bool HasRenderingRules(RenderingDefinition definition)
        {
            if (definition.Rules == null)
            {
                return false;
            }

            RulesDefinition def = new RulesDefinition(definition.Rules.ToString());
            foreach (XElement rule in def.GetRules().Where(rule => rule.Attribute("uid").Value != ItemIDs.Null.ToString()))
            {
                var actions = rule.Descendants("actions").FirstOrDefault();

                if (actions != null && actions.Descendants().Any())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}