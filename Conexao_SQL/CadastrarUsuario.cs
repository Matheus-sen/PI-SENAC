using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Conexao_SQL
{
    public partial class CadastrarUsuario : Form
    {
        MySqlConnection Conexao;

        public int IdUsuario = 1;
        public string data_source = "datasource=LOCALHOST;username=root;password=;database=Atividade_Conexao";

        public CadastrarUsuario()
        {
            InitializeComponent();
            CarregarProximoIdBancoUsuario();
        }

        private void Limpar()
        {
            txtUserNovo.Clear();
            txtSenhaNova.Clear();
            cbxTipoUsuario.SelectedIndex = -1;
        }

        public void Erro(string mensagem)
        {
            MessageBox.Show(mensagem, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void Sucesso(string mensagem)
        {
            MessageBox.Show(mensagem, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CarregarProximoIdBancoUsuario()
        {
            Conexao = new MySqlConnection(data_source);
            try
            {
                Conexao.Open();
                string selectIdUsuBanco = "SELECT MAX(id_usuario) FROM login";
                MySqlCommand selectbanc = new MySqlCommand(selectIdUsuBanco, Conexao);
                object resultadoMax = selectbanc.ExecuteScalar();

                if (resultadoMax != DBNull.Value && resultadoMax != null)
                {
                    IdUsuario = Convert.ToInt32(resultadoMax) + 1;
                }
                else
                {
                    IdUsuario = 1;
                }

            }
            catch (MySqlException ex)
            {
                Erro($"Erro ao carregar o próximo ID: {ex.Message}");
            }
            finally
            {
                Conexao.Close();
            }

        }

        private void CadastrarUsuário_Load(object sender, EventArgs e)
        {
            txtIdUsuarioCadastrado.Text = IdUsuario.ToString();
            txtIdUsuarioCadastrado.Enabled = false;
        }

        private void btnCadastrarUsuario_Click(object sender, EventArgs e)
        {
            Conexao = new MySqlConnection(data_source);
            Conexao.Open();
            MySqlCommand prodcmd = new MySqlCommand();
            prodcmd.Connection = Conexao;




            if (string.IsNullOrEmpty(txtUserNovo.Text) || string.IsNullOrEmpty(txtSenhaNova.Text) || string.IsNullOrEmpty(cbxTipoUsuario.Text))
            {
                Erro("Não pode conter campos vazios!");
                return;
            }
            else
            {
                prodcmd.Parameters.Clear(); // limpa os parâmetros antigos
                prodcmd.CommandText =
                    "INSERT INTO login " +
                    "(user, senha, tipo_usuario) " +
                    "VALUES " +
                    "(@user, @senha, @tipo_usuario)";

                prodcmd.Parameters.AddWithValue("@user", txtUserNovo.Text);
                prodcmd.Parameters.AddWithValue("@senha", txtSenhaNova.Text);
                prodcmd.Parameters.AddWithValue("@tipo_usuario", cbxTipoUsuario.SelectedItem.ToString()); // Pega o item selecionado

                prodcmd.ExecuteNonQuery();
                Sucesso("Usuário Cadastrado com sucesso!");
                IdUsuario++; // Incrementa o próximo ID
                txtIdUsuarioCadastrado.Text = IdUsuario.ToString(); // Atualiza o campo de ID
                Limpar();

            }
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            Conexao = new MySqlConnection(data_source);
            try
            {
                Conexao.Open();
                string verificarAdmin = "SELECT COUNT(*) FROM login WHERE tipo_usuario = 'Administrador'";
                MySqlCommand verificarCmd = new MySqlCommand(verificarAdmin, Conexao);
                int countAdmin = Convert.ToInt32(verificarCmd.ExecuteScalar());

                if (countAdmin == 0)
                {
                    MessageBox.Show("É necessário cadastrar pelo menos um usuário Administrador antes de voltar!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    this.Close();
                }
            }
            catch (MySqlException ex)
            {
                Erro($"Erro ao verificar usuários Administradores: {ex.Message}");
            }
            finally
            {
                Conexao.Close();
            }
        }
    }
}

