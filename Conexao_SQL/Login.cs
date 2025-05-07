using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace Conexao_SQL
{
    public partial class Login : Form
    {
        MySqlConnection Conexao;

        public string data_source = "datasource=LOCALHOST;username=root;password=;database=Atividade_Conexao";

        public Login()
        {
            InitializeComponent();
            UsuarioPadrao();
        }

        private void Limpar_Form()
        {
            txtUser.Clear();
            txtSenha.Clear();
        }

        private void txtUser_Enter(object sender, EventArgs e)
        {

            //if (txtUser.Text == "User/Login...")
            //{
            //    txtUser.Text = "";
            //    txtUser.ForeColor = Color.Black;
            //}

        }

        private void txtUser_Leave(object sender, EventArgs e)
        {

            //if (txtUser.Text == "")
            //{
            //    txtUser.Text = "User/Login...";
            //    txtUser.ForeColor = Color.Gray;
            //}

        }

        private void Login_Load(object sender, EventArgs e)
        {

            //txtUser.Text = "User/Login...";
            //txtUser.ForeColor = Color.Gray;

            //txtSenha.Text = "Senha...";
            //txtSenha.ForeColor = Color.Gray;



        }

        private void txtSenha_Enter(object sender, EventArgs e)
        {
            //if (txtSenha.Text == "Senha...")
            //{
            //    txtSenha.Text = "";
            //    txtSenha.ForeColor = Color.Black;
            //    txtSenha.PasswordChar = '*';

            //}
        }

        private void txtSenha_Leave(object sender, EventArgs e)
        {
            //if (txtSenha.Text == "")
            //{
            //    txtSenha.Text = "Senha...";
            //    txtSenha.ForeColor = Color.Gray;
            //}
        }

 
        private void UsuarioPadrao()
        {
            Conexao = new MySqlConnection(data_source);

            try
            {
                Conexao.Open();
                string verificarAdmin = "SELECT COUNT(*) FROM login WHERE tipo_usuario = 'Padrão'";
                MySqlCommand verificarCmd = new MySqlCommand(verificarAdmin, Conexao);
                int count = Convert.ToInt32(verificarCmd.ExecuteScalar());


                if (count == 0)
                {
                    string inserirUsuarioPadrao = "INSERT INTO login (user, senha, tipo_usuario) VALUES ('admin', 'admin', 'Padrão')";
                    MySqlCommand insertcmd = new MySqlCommand(inserirUsuarioPadrao, Conexao);
                    insertcmd.ExecuteNonQuery();
                    MessageBox.Show("Usuário administrador padrão criado com sucesso!", "Primeiro Acesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtUser.Text = "admin";
                    txtUser.ForeColor = Color.Black; // Opcional: ajustar a cor do texto
                    txtSenha.Text = "admin";
                    txtSenha.ForeColor = Color.Black; // Opcional: ajustar a cor do texto
                    txtSenha.PasswordChar = '*'; // Opcional: mostrar a senha como asteriscos

                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Erro ao criar usuário padrão: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Conexao.Close();
            }
        }

        private void btnLogar_Click(object sender, EventArgs e)
        {
            string usuarioDigitado = txtUser.Text.Trim().ToUpper();
            string senhaDigitada = txtSenha.Text;

            if (string.IsNullOrEmpty(usuarioDigitado))
            {
                MessageBox.Show("Campo 'User/Login...' precisa estar preenchido","Aviso",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUser.Focus();
                return;
            }

            if (string.IsNullOrEmpty(senhaDigitada))
            {
                MessageBox.Show("Campo 'Senha...' precisa estar preenchido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSenha.Focus();
                return;
            }

            Conexao = new MySqlConnection(data_source);

            try
            {
                Conexao.Open();

                string selectBanco = "SELECT tipo_usuario FROM login WHERE user = @user AND senha = @senha";
                MySqlCommand cmdTipoU = new MySqlCommand(selectBanco, Conexao);
                cmdTipoU.Parameters.AddWithValue("@user", usuarioDigitado);
                cmdTipoU.Parameters.AddWithValue("@senha", senhaDigitada);

                string tipoUsuario = (string)cmdTipoU.ExecuteScalar();

                if (tipoUsuario == "Administrador")
                {
                    Limpar_Form();
                    Menu menuForm = new Menu();
                    menuForm.ShowDialog();
                }
                else if (tipoUsuario == "Simples")
                {
                    Limpar_Form();
                    Visualizar visualizarForm = new Visualizar(tipoUsuario);
                    visualizarForm.ShowDialog();
                }
                else if (tipoUsuario == "Padrão")
                {
                    Limpar_Form();
                    CadastrarUsuario cadastrarusuarioForm = new CadastrarUsuario();
                    cadastrarusuarioForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Usuário ou senha incorretos.", "Erro de Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUser.Focus();
                    Limpar_Form();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Erro ao conectar ou consultar o banco de dados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Conexao.Close();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

}


//1° - Precisa fazer o cadastro do usuário padrão e verificar se ele já existe;

//2º - Precisa fazer o método para quando iniciar o form outra vez, ele logar com ou o mesmo admin ou com o novo usuário e tipo cadastrado, fazendo com o que inicie o form visualizar caso seja simples e menu em diante caso seja administrador;