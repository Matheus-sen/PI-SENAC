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
using Mysqlx.Crud;

namespace Conexao_SQL
{
    public partial class Visualizar : Form
    {

        public string data_source = "datasource=LOCALHOST;username=root;password=;database=Atividade_Conexao";
        MySqlConnection Conexao;

        public int? id_produto_selecionado = null;

        public Visualizar(string tipoUsuario)
        {
            InitializeComponent();
            string usuarioLogado = tipoUsuario;
            if (usuarioLogado == "Simples")
            {
                lblVerificacao.Text = usuarioLogado.ToUpper();
                lblVerificacao.ForeColor = Color.Green;
            }

            lstVisualizar.View = View.Details;//exibe as linhas das colunas e linhas
            lstVisualizar.LabelEdit = true;
            lstVisualizar.AllowColumnReorder = true; //mexe na ordem das colunas
            lstVisualizar.FullRowSelect = true; //selecionar linha completa
            lstVisualizar.GridLines = true;


            lstVisualizar.Columns.Add("ID Produto", 65, HorizontalAlignment.Left);
            lstVisualizar.Columns.Add("Produto", 250, HorizontalAlignment.Left);
            lstVisualizar.Columns.Add("Categoria", 200, HorizontalAlignment.Left);
            lstVisualizar.Columns.Add("Valor", 85, HorizontalAlignment.Left);
            lstVisualizar.Columns.Add("Un.Medida", 65, HorizontalAlignment.Left);
            lstVisualizar.Columns.Add("Localização", 200, HorizontalAlignment.Left);
            lstVisualizar.Columns.Add("Descrição", 450, HorizontalAlignment.Left);
            carregar_produtos();
            CarregarCategoriasNoComboBox();

            if (usuarioLogado == "Simples")
            {
                btnExcluir.Visible = false;
                btnAtualizar.Visible = false;
                btnSair.Location = new System.Drawing.Point(27, 380);

            }
            else
            {
                btnExcluir.Visible = true;
                btnAtualizar.Visible = true;
            }
        }
        private void Limpar()
        {
            txtIdProduto.Clear();
            txtNomeProduto.Clear();
            cbxCategoriaProduto.SelectedIndex = -1;
            txtValor.Clear();
            cbxUnidadeControle.SelectedIndex = -1;
            txtLocalizacao.Clear();
            txtDescricao.Clear();
        }

        public void Erro(string mensagem)
        {
            MessageBox.Show(mensagem, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void Sucesso(string mensagem)
        {
            MessageBox.Show(mensagem, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void carregar_produtos()
        {
            try
            {

                // Criar a conexão com o MySQL
                Conexao = new MySqlConnection(data_source);

                string info = "SELECT p.id_produto, p.nome_produto, c.nome_categoria, p.valor, p.unidade_controle, p.localizacao, p.descricao " +
                              "FROM produto p " +
                              "INNER JOIN categoria c ON p.id_categoria = c.id_categoria";

                Conexao.Open();

                // Buscar as informações
                MySqlCommand buscar = new MySqlCommand(info, Conexao);

                // armazena as informacoes que temos na busca para mostrar na tela
                MySqlDataReader reader = buscar.ExecuteReader();

                // como iremos mostrar os dados na tela para o usuário
                lstVisualizar.Items.Clear();

                while (reader.Read())
                {
                    string[] row =
                    {
                        // obtendo as informações do banco de dados (vetor de strings)
                        reader.GetInt32(0).ToString(), // id produto
                        reader.GetString(1),           // nome produto
                        reader.GetString(2),           // categoria produto
                        reader.GetString(3),           // valor
                        reader.GetString(4),           // Unidade de medida
                        reader.GetString(5),           // Localizacao
                        reader.GetString(6),           // Descricao
                    };

                    var linha_list_view = new ListViewItem(row);
                    lstVisualizar.Items.Add(linha_list_view);
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

        private void lstVisualizar_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListView.SelectedListViewItemCollection itens_selecionados = lstVisualizar.SelectedItems;

            // percorrendo a coleção de itens dentro da lista itens_selecionados
            // Obs¹: A minha linha toda é um item, que contem os subItems (colunas) que desejo selecionar

            foreach (ListViewItem item in itens_selecionados)
            {
                id_produto_selecionado = Convert.ToInt32(item.SubItems[0].Text);
                CarregarDetalhesProduto((int)id_produto_selecionado); // Chama o método para carregar os detalhes
            }

        }

        private void excluir_contato()
        {
            if (id_produto_selecionado != null)
            {
                try
                {

                    DialogResult conf = MessageBox.Show("Deseja Excluir o Registro de ID " + id_produto_selecionado + " ?",
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
                        cmd.CommandText = "DELETE FROM produto WHERE id_produto=@id_produto";
                        cmd.Parameters.AddWithValue("@id_produto", id_produto_selecionado);

                        cmd.ExecuteNonQuery();


                        MessageBox.Show(
                                "Produto Excluido com Sucesso!",
                                "Sucesso", MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                                );


                        carregar_produtos();

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
                Erro($"Nenhum item/ produto foi selecionado, selecione um item para excluir!");
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

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            excluir_contato();
        }

        private void CarregarDetalhesProduto(int idProduto)
        {
            try
            {
                Conexao = new MySqlConnection(data_source);
                Conexao.Open();

                string query = "SELECT p.id_produto, p.nome_produto, c.nome_categoria, p.valor, p.unidade_controle, p.localizacao, p.descricao " +
                               "FROM produto p " +
                               "INNER JOIN categoria c ON p.id_categoria = c.id_categoria " +
                               "WHERE p.id_produto = @id_produto";

                MySqlCommand cmd = new MySqlCommand(query, Conexao);
                cmd.Parameters.AddWithValue("@id_produto", idProduto);

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtIdProduto.Text = reader.GetInt32(0).ToString();
                    txtNomeProduto.Text = reader.GetString(1);
                    cbxCategoriaProduto.SelectedItem = reader.GetString(2);
                    txtValor.Text = reader.GetString(3);
                    cbxUnidadeControle.SelectedItem = reader.GetString(4);
                    txtLocalizacao.Text = reader.GetString(5);
                    txtDescricao.Text = reader.GetString(6);
                }
                else
                {
                    Erro("Produto não encontrado!");
                }
            }
            catch (Exception ex)
            {
                Erro($"Erro ao carregar detalhes do produto: {ex.Message}");
            }
            finally
            {
                Conexao.Close();
            }
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {

            try
            {

                Conexao = new MySqlConnection(data_source);
                Conexao.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Conexao;

                string nomeCategoriaSelecionada = cbxCategoriaProduto.SelectedItem?.ToString();


                if (id_produto_selecionado != null)
                {
                    // Busca o ID da categoria com base no nome selecionado no ComboBox
                    string selectIdCategoria = "SELECT id_categoria FROM categoria WHERE nome_categoria = @nome_categoria";
                    MySqlCommand cmdSelectCategoria = new MySqlCommand(selectIdCategoria, Conexao);
                    cmdSelectCategoria.Parameters.AddWithValue("@nome_categoria", nomeCategoriaSelecionada);
                    object categoriaIdResult = cmdSelectCategoria.ExecuteScalar();

                    cmd.Parameters.Clear(); // limpa os parâmetros antigos
                    cmd.CommandText =
                        "UPDATE produto " +
                        "SET nome_produto = @nome_produto, id_categoria = @id_categoria, valor = @valor, unidade_controle = @unidade_controle, localizacao = @localizacao, descricao = @descricao WHERE id_produto = @id_produto";


                    cmd.Parameters.AddWithValue("@id_produto", id_produto_selecionado);
                    cmd.Parameters.AddWithValue("@nome_produto", txtNomeProduto.Text);
                    cmd.Parameters.AddWithValue("@id_categoria", Convert.ToInt32(categoriaIdResult));
                    cmd.Parameters.AddWithValue("@valor", txtValor.Text);
                    cmd.Parameters.AddWithValue("@unidade_controle", cbxUnidadeControle.Text);
                    cmd.Parameters.AddWithValue("@localizacao", txtLocalizacao.Text);
                    cmd.Parameters.AddWithValue("@descricao", txtDescricao.Text);


                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Produto Atualizado com Sucesso", "Sucesso",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                    carregar_produtos();
                    Limpar();
                }
                else
                {
                    MessageBox.Show("Selecione um item para ser atualizado! ", "Error ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


            }
            catch (Exception ex)
            {
                string errorMessage = "Ocorreu um erro: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " - Inner Exception: " + ex.InnerException.Message;
                }
                MessageBox.Show(errorMessage, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Conexao.Close();
            }
        }

        private void txtSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnFiltar_Click(object sender, EventArgs e)
        {
            try
            {
                // Criar a conexão com o MySQL
                Conexao = new MySqlConnection(data_source);
                Conexao.Open();

                string filtro = txtFiltrar.Text.ToLower(); // Obtém o texto de filtro e converte para minúsculo
                lstVisualizar.Items.Clear(); // Limpa a lista antes de filtrar

                string info = "SELECT p.id_produto, p.nome_produto, c.nome_categoria, p.valor, p.unidade_controle, p.localizacao, p.descricao " +
                            "FROM produto p " +
                            "INNER JOIN categoria c ON p.id_categoria = c.id_categoria";

                MySqlCommand buscar = new MySqlCommand(info, Conexao);
                MySqlDataReader reader = buscar.ExecuteReader();

                while (reader.Read())
                {
                    string nomeProduto = reader.GetString(1).ToLower();
                    string nomeCategoria = reader.GetString(2).ToLower();

                    // Verifica se o texto de filtro está contido no nome do produto ou na categoria
                    if (nomeProduto.Contains(filtro) || nomeCategoria.Contains(filtro))
                    {
                        string[] row =
                        {
                            reader.GetInt32(0).ToString(),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4),
                            reader.GetString(5),
                            reader.GetString(6)
                        };
                        var linha_list_view = new ListViewItem(row);
                        lstVisualizar.Items.Add(linha_list_view);
                    }
                    Limpar();
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

    }
}
