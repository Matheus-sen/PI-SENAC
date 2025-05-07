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
    public partial class cadastrarProduto: Form
    {
        public int IdProduto = 1;
        public string data_source = "datasource=LOCALHOST;username=root;password=;database=Atividade_Conexao";
        MySqlConnection Conexao;
        public cadastrarProduto()
        {
            InitializeComponent();
            CarregarProximoIdDoBanco();
            CarregarCategoriasNoComboBox();
        }

        private void Limpar()
        {
            txtNomeProduto.Clear();
            cbxCategoriaProduto.SelectedIndex = -1;
            txtValor.Clear();
            cbxUnidadeControle.SelectedIndex = -1;
            txtLocalizacao.Clear();
            txtDescricao.Clear();
        }
        private void CarregarProximoIdDoBanco()
        {
            Conexao = new MySqlConnection(data_source);
            try
            {
                Conexao.Open();
                string selectIdProdBanco = "SELECT MAX(id_produto) FROM produto";
                MySqlCommand selectprodbanc = new MySqlCommand(selectIdProdBanco, Conexao);
                object resultadoMax = selectprodbanc.ExecuteScalar();

                if (resultadoMax != DBNull.Value && resultadoMax != null)
                {
                    IdProduto = Convert.ToInt32(resultadoMax) + 1;
                }
                else
                {
                    IdProduto = 1;
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

        public void CarregarCategoriasNoComboBox()
        {
            Conexao = new MySqlConnection(data_source);
            try
            {
                Conexao.Open();
                string selectCategorias = "SELECT nome_categoria FROM categoria";
                MySqlCommand cmdCategorias = new MySqlCommand(selectCategorias, Conexao);
                MySqlDataReader readerCategorias = cmdCategorias.ExecuteReader();

                cbxCategoriaProduto.Items.Clear(); // Limpa os itens existentes no ComboBox
                while (readerCategorias.Read())
                {
                    cbxCategoriaProduto.Items.Add(readerCategorias.GetString(0)); // Adiciona cada nome de categoria ao ComboBox
                }
            }
            catch (MySqlException ex)
            {
                Erro($"Erro ao carregar categorias no ComboBox: {ex.Message}");
            }
            finally
            {
                Conexao.Close();
            }
        }


        public void Erro(string mensagem)
        {
            MessageBox.Show(mensagem, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void Sucesso(string mensagem)
        {
            MessageBox.Show(mensagem, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            
            Conexao = new MySqlConnection(data_source);
            Conexao.Open();
            MySqlCommand prodcmd = new MySqlCommand();
            prodcmd.Connection = Conexao;




            if (string.IsNullOrEmpty(txtNomeProduto.Text) || string.IsNullOrEmpty(cbxCategoriaProduto.Text) || string.IsNullOrEmpty(cbxUnidadeControle.Text))
            {
                Erro("Campos com '*' são de preenchimentos obrigatório!");
                return;
            }
            else
            {
                prodcmd.Parameters.Clear(); // limpa os parâmetros antigos
                prodcmd.CommandText =
                    "INSERT INTO produto " +
                    "(nome_produto, id_categoria, valor, unidade_controle, localizacao, descricao ) " +
                    "VALUES " +
                    "(@nome_produto,(SELECT id_categoria FROM categoria WHERE nome_categoria = @nome) ,@valor, @unidade_controle, @localizacao, @descricao )";

                prodcmd.Parameters.AddWithValue("@nome_produto", txtNomeProduto.Text);
                prodcmd.Parameters.AddWithValue("@nome", cbxCategoriaProduto.SelectedItem.ToString()); // Pega o item selecionado
                prodcmd.Parameters.AddWithValue("@valor", txtValor.Text);
                prodcmd.Parameters.AddWithValue("@unidade_controle", cbxUnidadeControle.SelectedItem.ToString()); // Pega o item selecionado
                prodcmd.Parameters.AddWithValue("@localizacao", txtLocalizacao.Text);
                prodcmd.Parameters.AddWithValue("@descricao", txtDescricao.Text);

                prodcmd.ExecuteNonQuery();
                Sucesso("Produto Cadastrado com sucesso!");
                IdProduto++; // Incrementa o próximo ID
                txtIdProduto.Text = IdProduto.ToString(); // Atualiza o campo de ID
                Limpar();

            }
        }

        private void cadastrarProduto_Load(object sender, EventArgs e)
        {
            txtIdProduto.Text = IdProduto.ToString();
            txtIdProduto.Enabled = false;
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
