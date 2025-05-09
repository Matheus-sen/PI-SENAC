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
        public int? id_usuario_selecionado = null;

        public CadastrarUsuario()
        {
            InitializeComponent();
            CarregarProximoIdBancoUsuario();

            lstUsuario.View = View.Details;//exibe as linhas das colunas e linhas
            lstUsuario.LabelEdit = true;
            lstUsuario.AllowColumnReorder = true; //mexe na ordem das colunas
            lstUsuario.FullRowSelect = true; //selecionar linha completa
            lstUsuario.GridLines = true;


            lstUsuario.Columns.Add("ID", 35, HorizontalAlignment.Left);
            lstUsuario.Columns.Add("User", 75, HorizontalAlignment.Left);
            lstUsuario.Columns.Add("Tipo", 50, HorizontalAlignment.Left);
            carregar_usuario();
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
                carregar_usuario();

            }
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            Conexao = new MySqlConnection(data_source);
            try
            {
                //Criando a validação pra retornar apenas se houver ao menos UM usuário Administrador:
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

        private void carregar_usuario()
        {
            try
            {
                Conexao = new MySqlConnection(data_source);

                string dadosUsuario = "SELECT id_usuario, user, tipo_usuario " +
                              "FROM login " +
                              "ORDER BY id_usuario ASC";

                Conexao.Open();

                // Buscar as informações
                MySqlCommand buscar = new MySqlCommand(dadosUsuario, Conexao);

                // armazena as informacoes que temos na busca para mostrar na tela
                MySqlDataReader reader = buscar.ExecuteReader();

                // como iremos mostrar os dados na tela para o usuário
                lstUsuario.Items.Clear();

                while (reader.Read())
                {
                    string[] row =
                    {
                        // obtendo as informações do banco de dados (vetor de strings)
                        reader.GetInt32(0).ToString(), // id usuário
                        reader.GetString(1),           // user
                        reader.GetString(2),           // tipo Usuário
                    };

                    var linha_list_view = new ListViewItem(row);
                    lstUsuario.Items.Add(linha_list_view);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Conexao.Close();
            }
        }

        private void ExcluirUsuário()
        {
            if (id_usuario_selecionado != null)
            {

                if (id_usuario_selecionado == 1)
                {
                    Erro("O usuário 'admin' do tipo 'Padrão' não pode ser excluído!");
                    return; // Impede a exclusão do admin
                }
                try
                {

                    DialogResult conf = MessageBox.Show("Deseja Excluir o Usuário de Registro de ID " + id_usuario_selecionado + " ?",
                                                        "Certeza ?",
                                                           MessageBoxButtons.YesNo,
                                                           MessageBoxIcon.Warning);

                    if (conf == DialogResult.Yes)
                    {


                        Conexao = new MySqlConnection(data_source);
                        Conexao.Open();
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = Conexao;

                        cmd.Connection = Conexao;
                        cmd.CommandText = "DELETE FROM login WHERE id_usuario=@id_usuario";
                        cmd.Parameters.AddWithValue("@id_usuario", id_usuario_selecionado);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show(
                                "Usuário Excluido com Sucesso!",
                                "Sucesso", MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                                );

                        carregar_usuario();

                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Erro " + ex.Number + "Ocorreu: " + ex.Message,
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                catch (Exception ex)
                {

                    MessageBox.Show("Ocorreu: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                finally
                {
                    Conexao.Close();
                }
            }
            else
            {
                Erro($"Nenhum usuário foi selecionado para exclusão!");
            }

        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            ExcluirUsuário();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection itens_selecionados = lstUsuario.SelectedItems;

            // percorrendo a coleção de itens dentro da lista itens_selecionados
            // Obs¹: A minha linha toda é um item, que contem os subItems (colunas) que desejo selecionar

            foreach (ListViewItem item in itens_selecionados)
            {
                id_usuario_selecionado = Convert.ToInt32(item.SubItems[0].Text);
            }
        }
    }
}

