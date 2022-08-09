// Decompiled with JetBrains decompiler
// Type: LDAPQueryUserTest.Form1
// Assembly: LDAPQueryUserTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EB2059A6-71AC-4D0F-B8F5-3AD585E3BC32
// Assembly location: D:\Tests\Tests\AD User Test\LDAPQueryUserTest.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LDAPQueryUserTest
{
  public class Form1 : Form
  {
    private IContainer components = (IContainer) null;
    private string istrMessage;
    private TextBox txtQueryUser;
    private TextBox txtQueryPassword;
    private TextBox txtUser;
    private Label lblUser;
    private Label lblPassword;
    private Label label3;
    private Button btnSubmit;
    private ListBox lstGroupdetails;
    private TextBox txtLdapDomain;
    private Label label1;
    private TextBox txtLDAPtbPath;
    private CheckBox chkUseCurrentDomain;
    private Label label2;
    private TextBox txtGroup;
    private Label lblConnectedServer;
    private Label lblResult;
    private ListBox lstUsersInGroups;
    private Label label4;
    private Button btnGetUsers;
    private Button button1;

    public Form1()
    {
      this.InitializeComponent();
    }

    private void chkUseCurrentDomain_CheckedChanged(object sender, EventArgs e)
    {
      if (this.chkUseCurrentDomain.Checked)
      {
        this.txtLDAPtbPath.Text = DomainManager.RootPath;
        this.txtLDAPtbPath.ReadOnly = true;
      }
      else
        this.txtLDAPtbPath.ReadOnly = false;
    }

    private void btnSubmit_Click(object sender, EventArgs e)
    {
      try
      {
        string text1 = this.txtLdapDomain.Text;
        string text2 = this.txtGroup.Text;
        string text3 = this.txtQueryUser.Text;
        string text4 = this.txtQueryPassword.Text;
        if (this.IsValidKerberosUser(text3, text4, text1, this.txtUser.Text))
        {
          this.CheckGroupAuthorization(text3, text4, text2, ref this.istrMessage);
          this.lblResult.Text = this.istrMessage;
        }
        else
        {
          int num = (int) MessageBox.Show(this.istrMessage);
        }
      }
      catch (Exception ex)
      {
        this.lblResult.Text += ex.Message;
        int num = (int) MessageBox.Show(ex.Message.ToString());
      }
    }

    private void CheckGroupAuthorization(
      string queryUser,
      string queryUserPassword,
      string groupName,
      ref string istrMessage)
    {
      SearchResult one = new DirectorySearcher(new DirectoryEntry(this.txtLDAPtbPath.Text, queryUser, queryUserPassword))
      {
        Filter = ("(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + this.txtUser.Text + "))"),
        SearchScope = SearchScope.Subtree
      }.FindOne();
      int count = one.Properties["memberOf"].Count;
      StringBuilder stringBuilder = new StringBuilder();
      List<string> source = new List<string>();
      for (int index = 0; index < count; ++index)
      {
        string str = (string) one.Properties["memberOf"][index];
        int num1 = str.IndexOf("=", 1);
        int num2 = str.IndexOf(",", 1);
        if (-1 != num1)
          ;
        source.Add(str.Substring(num1 + 1).Substring(0, num2 - num1 - 1));
      }
      bool flag = source.Any<string>((Func<string, bool>) (x => Convert.ToString(x).Trim() == Convert.ToString(groupName)));
      istrMessage = flag ? istrMessage : istrMessage + " but no group access";
      this.lstGroupdetails.Items.Clear();
      foreach (object obj in source)
        this.lstGroupdetails.Items.Add(obj);
    }

    private bool IsValidKerberosUser(
      string istrQueryUserId,
      string istrQueryUserPassword,
      string astrDomainName,
      string istrUserToSearch)
    {
      PrincipalContext context = new PrincipalContext(ContextType.Domain, astrDomainName, istrQueryUserId, istrQueryUserPassword);
      this.lblConnectedServer.Text = "Connected Server: " + context.ConnectedServer;
      UserPrincipal byIdentity = UserPrincipal.FindByIdentity(context, istrUserToSearch);
      if (byIdentity == null)
      {
        this.istrMessage = "ERROR-AD001: User entry not found in Active Directory. Check ADS path.";
        return false;
      }
      if (byIdentity.IsAccountLockedOut())
      {
        this.istrMessage = "ERROR-AD002: User account is locked in Active Directory.";
        return false;
      }
      this.istrMessage = "Successful Authentication";
      this.lblResult.Text = this.istrMessage;
      return true;
    }

    private void btnGetUsers_Click(object sender, EventArgs e)
    {
           // MessageBox.Show(Path.GetDirectoryName(Application.ExecutablePath));
            this.lstUsersInGroups.Items.Clear();
      foreach (object disabledUsername in this.GetDisabledUsernames(this.txtLdapDomain.Text, ((IEnumerable<string>) this.txtGroup.Text.Split(',')).ToList<string>(), this.txtQueryUser.Text, this.txtQueryPassword.Text))
        this.lstUsersInGroups.Items.Add(disabledUsername);
            string result = string.Join(",", lstUsersInGroups.Items.ToString());
           
            using (StreamWriter myOutputStream = new StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + "\\file"+ DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt"))
            {
                foreach (var item in lstUsersInGroups.Items)
                {
                    myOutputStream.WriteLine(item.ToString());
                }
            }
           // File.AppendAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\file.txt", lstUsersInGroups.Items.ToString() + Environment.NewLine);
        }

    public SortedSet<string> GetUsernames(string domainName, string groupName)
    {
      using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName))
      {
        using (GroupPrincipal byIdentity = GroupPrincipal.FindByIdentity(context, groupName))
          return byIdentity == null ? (SortedSet<string>) null : new SortedSet<string>(byIdentity.GetMembers(true).Select<Principal, string>((Func<Principal, string>) (u => u.SamAccountName)));
      }
    }

    public SortedSet<string> GetDisabledUsernames(
      string domainName,
      List<string> groupName,
      string istrQueryUserId,
      string istrQueryUserPassword)
    {
      SortedSet<string> strSortedUsers = new SortedSet<string>();
      for (int index = 0; index < groupName.Count; ++index)
      {
        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName, istrQueryUserId, istrQueryUserPassword))
        {
          using (GroupPrincipal byIdentity = GroupPrincipal.FindByIdentity(pc, groupName[index]))
          {
                        var test1 = byIdentity.GetMembers(true);
                        foreach (UserPrincipal p in test1)
                        {
                            if (p != null)
                                strSortedUsers.Add(p.SamAccountName);
                        }
                       // if (byIdentity != null)

            //((IEnumerable<string>) byIdentity.GetMembers(false).Select<Principal, string>((Func<Principal, string>) (u => u.SamAccountName)).ToArray<string>()).ToList<string>().ForEach((Action<string>) (x =>
            //{
            //  if (strSortedUsers.Contains(x))
            //    return;
            //  bool? enabled = UserPrincipal.FindByIdentity(pc, x).Enabled;
            //  bool flag = true;
            //  if (enabled.GetValueOrDefault() == flag & enabled.HasValue)
            //    strSortedUsers.Add(x);
            //}));
          }
        }
      }
      return strSortedUsers;
    }

    private bool IsUserDisabledInAD(
      string istrQueryUserId,
      string istrQueryUserPassword,
      string astrDomainName,
      string istrUserToSearch)
    {
      UserPrincipal byIdentity = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, astrDomainName, istrQueryUserId, istrQueryUserPassword), istrUserToSearch);
      if (byIdentity == null)
        return true;
      bool? enabled = byIdentity.Enabled;
      bool flag = false;
      return enabled.GetValueOrDefault() == flag & enabled.HasValue;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      ((IEnumerable<string>) this.txtGroup.Text.Split(',')).ToList<string>();
      foreach (string str in this.GetUsernamesLike(this.txtLdapDomain.Text, this.txtUser.Text))
      {
        if (!string.IsNullOrEmpty(str))
          this.lstUsersInGroups.Items.Add((object) str);
      }
    }

    public SortedSet<string> GetUsernamesLike(string domain, string istrUserid)
    {
      SortedSet<string> sortedSet = new SortedSet<string>();
      using (PrincipalSearcher principalSearcher = new PrincipalSearcher((Principal) new UserPrincipal(new PrincipalContext(ContextType.Domain, domain, this.txtQueryUser.Text, this.txtQueryPassword.Text))))
      {
        foreach (UserPrincipal userPrincipal in principalSearcher.FindAll().Select<Principal, UserPrincipal>((Func<Principal, UserPrincipal>) (u => (UserPrincipal) u)).ToList<UserPrincipal>())
          sortedSet.Add(userPrincipal.DisplayName);
      }
      return sortedSet;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.txtQueryUser = new System.Windows.Forms.TextBox();
            this.txtQueryPassword = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lstGroupdetails = new System.Windows.Forms.ListBox();
            this.txtLdapDomain = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLDAPtbPath = new System.Windows.Forms.TextBox();
            this.chkUseCurrentDomain = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGroup = new System.Windows.Forms.TextBox();
            this.lblConnectedServer = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.lstUsersInGroups = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnGetUsers = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtQueryUser
            // 
            this.txtQueryUser.Location = new System.Drawing.Point(233, 58);
            this.txtQueryUser.Margin = new System.Windows.Forms.Padding(4);
            this.txtQueryUser.Name = "txtQueryUser";
            this.txtQueryUser.Size = new System.Drawing.Size(243, 22);
            this.txtQueryUser.TabIndex = 2;
            // 
            // txtQueryPassword
            // 
            this.txtQueryPassword.Location = new System.Drawing.Point(233, 90);
            this.txtQueryPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtQueryPassword.Name = "txtQueryPassword";
            this.txtQueryPassword.PasswordChar = '*';
            this.txtQueryPassword.Size = new System.Drawing.Size(243, 22);
            this.txtQueryPassword.TabIndex = 3;
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(233, 122);
            this.txtUser.Margin = new System.Windows.Forms.Padding(4);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(243, 22);
            this.txtUser.TabIndex = 4;
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(132, 65);
            this.lblUser.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(96, 17);
            this.lblUser.TabIndex = 3;
            this.lblUser.Text = "Query User Id";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(83, 97);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(146, 17);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "Query User Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(152, 126);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "AD User ";
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(257, 324);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(4);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(100, 28);
            this.btnSubmit.TabIndex = 7;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // lstGroupdetails
            // 
            this.lstGroupdetails.FormattingEnabled = true;
            this.lstGroupdetails.ItemHeight = 16;
            this.lstGroupdetails.Location = new System.Drawing.Point(663, 27);
            this.lstGroupdetails.Margin = new System.Windows.Forms.Padding(4);
            this.lstGroupdetails.Name = "lstGroupdetails";
            this.lstGroupdetails.Size = new System.Drawing.Size(289, 292);
            this.lstGroupdetails.TabIndex = 23;
            // 
            // txtLdapDomain
            // 
            this.txtLdapDomain.Location = new System.Drawing.Point(233, 165);
            this.txtLdapDomain.Margin = new System.Windows.Forms.Padding(4);
            this.txtLdapDomain.Name = "txtLdapDomain";
            this.txtLdapDomain.Size = new System.Drawing.Size(243, 22);
            this.txtLdapDomain.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(168, 165);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 25;
            this.label1.Text = "Domain";
            // 
            // txtLDAPtbPath
            // 
            this.txtLDAPtbPath.Location = new System.Drawing.Point(233, 27);
            this.txtLDAPtbPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtLDAPtbPath.Name = "txtLDAPtbPath";
            this.txtLDAPtbPath.Size = new System.Drawing.Size(243, 22);
            this.txtLDAPtbPath.TabIndex = 0;
            // 
            // chkUseCurrentDomain
            // 
            this.chkUseCurrentDomain.AutoSize = true;
            this.chkUseCurrentDomain.Location = new System.Drawing.Point(485, 30);
            this.chkUseCurrentDomain.Margin = new System.Windows.Forms.Padding(4);
            this.chkUseCurrentDomain.Name = "chkUseCurrentDomain";
            this.chkUseCurrentDomain.Size = new System.Drawing.Size(158, 21);
            this.chkUseCurrentDomain.TabIndex = 1;
            this.chkUseCurrentDomain.Text = "Use Current Domain";
            this.chkUseCurrentDomain.UseVisualStyleBackColor = true;
            this.chkUseCurrentDomain.CheckedChanged += new System.EventHandler(this.chkUseCurrentDomain_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(171, 204);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 17);
            this.label2.TabIndex = 29;
            this.label2.Text = "Group";
            // 
            // txtGroup
            // 
            this.txtGroup.Location = new System.Drawing.Point(236, 204);
            this.txtGroup.Margin = new System.Windows.Forms.Padding(4);
            this.txtGroup.Name = "txtGroup";
            this.txtGroup.Size = new System.Drawing.Size(243, 22);
            this.txtGroup.TabIndex = 6;
            // 
            // lblConnectedServer
            // 
            this.lblConnectedServer.AutoSize = true;
            this.lblConnectedServer.Location = new System.Drawing.Point(152, 279);
            this.lblConnectedServer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectedServer.Name = "lblConnectedServer";
            this.lblConnectedServer.Size = new System.Drawing.Size(126, 17);
            this.lblConnectedServer.TabIndex = 30;
            this.lblConnectedServer.Text = "Connected Server:";
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(156, 260);
            this.lblResult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(52, 17);
            this.lblResult.TabIndex = 31;
            this.lblResult.Text = "Result:";
            // 
            // lstUsersInGroups
            // 
            this.lstUsersInGroups.FormattingEnabled = true;
            this.lstUsersInGroups.ItemHeight = 16;
            this.lstUsersInGroups.Location = new System.Drawing.Point(1035, 39);
            this.lstUsersInGroups.Margin = new System.Windows.Forms.Padding(4);
            this.lstUsersInGroups.Name = "lstUsersInGroups";
            this.lstUsersInGroups.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lstUsersInGroups.Size = new System.Drawing.Size(289, 292);
            this.lstUsersInGroups.TabIndex = 32;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1041, 9);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 17);
            this.label4.TabIndex = 33;
            this.label4.Text = "Users in the group";
            // 
            // btnGetUsers
            // 
            this.btnGetUsers.Location = new System.Drawing.Point(1035, 339);
            this.btnGetUsers.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetUsers.Name = "btnGetUsers";
            this.btnGetUsers.Size = new System.Drawing.Size(100, 28);
            this.btnGetUsers.TabIndex = 8;
            this.btnGetUsers.Text = "Get Users in group";
            this.btnGetUsers.UseVisualStyleBackColor = true;
            this.btnGetUsers.Click += new System.EventHandler(this.btnGetUsers_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1035, 387);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(143, 28);
            this.button1.TabIndex = 34;
            this.button1.Text = "Get Users start with";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1337, 702);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnGetUsers);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lstUsersInGroups);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblConnectedServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtGroup);
            this.Controls.Add(this.chkUseCurrentDomain);
            this.Controls.Add(this.txtLDAPtbPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLdapDomain);
            this.Controls.Add(this.lstGroupdetails);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.txtQueryPassword);
            this.Controls.Add(this.txtQueryUser);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

    }
  }
}
