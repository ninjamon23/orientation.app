<template>
  <div class="container" v-if="$auth.isAuthenticated" style="padding: 10px">
    <a
      href="javascript:void(0)"
      class="button is-floating is-medium"
      title="Add New Hire"
      @click="showModal"
    >
      <i class="fas fa-user-plus"></i>
    </a>

    <div class="modal" :class="{ 'is-active': showFormModal }">
      <div class="modal-background"></div>
      <div class="modal-content">
        <form class="box" @submit.prevent="submitRecord">
          <h2 class="title">New Hire Form</h2>
          <div class="field">
            <label class="label">Category</label>
            <div class="control">
              <div class="select">
                <select v-model="form.category" required>
                  <option v-for="cat in categories" :key="cat.name">
                    {{ cat.name }}
                  </option>
                </select>
              </div>
            </div>
          </div>
          <div class="field">
            <label class="label">Employee No.</label>
            <div class="control">
              <input
                class="input"
                type="text"
                placeholder=""
                v-model="form.empNo"
                required
              />
            </div>
          </div>
          <div class="field">
            <label class="label">Name</label>
            <div class="control">
              <input
                class="input"
                type="text"
                placeholder="Eg. Jon Doe"
                v-model="form.name"
                required
              />
            </div>
          </div>

          <div class="field">
            <label class="label">Email</label>
            <div class="control">
              <input
                class="input"
                type="email"
                placeholder="Employee Email"
                v-model="form.email"
                required
              />
            </div>
          </div>

          <div class="field is-grouped">
            <div class="control">
              <button class="button is-link" type="submit">Submit</button>
            </div>
            <div class="control">
              <button class="button is-link is-light" @click="closeModal">
                Cancel
              </button>
            </div>
          </div>
        </form>
      </div>
      <button
        class="modal-close is-large"
        aria-label="close"
        @click="closeModal"
      ></button>
    </div>

    <h2 class="title">New Hires</h2>
    <div class="field has-addons">
      <div class="control">
        <input
          class="input"
          type="text"
          placeholder="Search record"
          v-model="paging.search"
        />
      </div>
      <div class="control">
        <div class="buttons">
          <a class="button is-info" @click="getNewHires"> Search </a>
          <a class="button is-info" @click="exportData"> Export </a>
        </div>
      </div>
    </div>
    <div class="table-container">
      <table
        class="table is-fullwidth is-striped is-hoverable"
        id="tblNewHires"
      >
        <thead>
          <tr>
            <th></th>
            <th>Employee #</th>
            <th>Email</th>
            <th>Name</th>
            <th>Category</th>
            <th>Status</th>
            <th>Date Created</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="record in newHires.result" :key="record.email">
            <td>
              <p class="buttons">
                <button class="button is-small" @click="update(record)">
                  <span class="icon is-small">
                    <i class="fas fa-edit"></i>
                  </span>
                </button>
                <button class="button is-small" @click="del(record.empNo)">
                  <span class="icon is-small">
                    <i class="fas fa-trash"></i>
                  </span>
                </button>
              </p>
            </td>
            <td>{{ record.empNo }}</td>
            <td>{{ record.email }}</td>
            <td>{{ record.name }}</td>
            <td>{{ record.category }}</td>
            <td>
              <!-- <span
                :class="{
                  'tag is-success': subRecord.isComplete,
                  'tag is-danger': !subRecord.isComplete,
                }"
                style="margin-right: 5px"
                v-for="subRecord in record.categoryWithDeck"
                :key="record.empNo + subRecord.name"
                >{{ subRecord.name }}</span
              > -->
              <div class="field is-grouped is-grouped-multiline">
                <div
                  class="control"
                  v-for="subRecord in record.categoryWithDeck"
                  :key="record.empNo + subRecord.name"
                >
                  <div class="tags has-addons">
                    <span
                      :class="{
                        'tag is-dark': subRecord.isComplete,
                        'tag is-danger': !subRecord.isComplete,
                      }"
                      >{{ subRecord.name }}</span
                    >
                    <span class="tag is-success" v-if="subRecord.isComplete">{{
                      subRecord.completionTime
                    }}</span>
                  </div>
                </div>
              </div>
            </td>
            <td>
              {{ record.creationDateTime }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div>
      <div class="select">
        <select v-model="paging.pageSize">
          <option v-for="pSize in pagingSizes" :key="pSize">{{ pSize }}</option>
        </select>
      </div>
    </div>
    <div>Page {{ paging.currentPage }} of {{ newHires.totalPage }}</div>
    <div class="buttons">
      <button
        class="button"
        @click="previousPage"
        v-if="paging.currentPage > 1"
      >
        Previous Page
      </button>
      <button
        class="button"
        @click="nextPage"
        v-if="paging.currentPage < newHires.totalPage"
      >
        Next Page
      </button>
    </div>
  </div>
</template>
<script>
import axios from 'axios'
import moment from 'moment'
export default {
  data () {
    return {
      isUpdate: false,
      showFormModal: false,
      categories: [],
      form: {
        empNo: '',
        name: '',
        email: '',
        category: '',
        categoryWithDeck: []
      },
      newHires: [],
      paging: {
        currentPage: 1,
        pageSize: 10,
        search: ''
      },
      pagingSizes: ['10', '50', '100', 'ALL']
    }
  },
  watch: {
    'paging.pageSize': function (val) {
      if (val) {
        this.getNewHires()
      }
    },
    'form.category': async function (val) {
      if (val) {
        const token = await this.$auth.getIdTokenClaims()
        this.$isLoading(true)
        this.form.categoryWithDeck = (
          await axios.get(
            'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/getbycategorywithdeck/' + val,
            {
              headers: {
                authorization: 'bearer ' + token.__raw
              }
            }
          )
        ).data
        this.$isLoading(false)
      }
    }
  },
  methods: {
    nextPage () {
      this.paging.currentPage = this.paging.currentPage + 1
      this.getNewHires()
    },
    previousPage () {
      this.paging.currentPage = this.paging.currentPage - 1
      this.getNewHires()
    },
    async del (empNo) {
      var r = confirm('Confirm Delete?')
      if (r === true) {
        const token = await this.$auth.getIdTokenClaims()
        await axios.delete(
          `https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/deletenewhire/${empNo}`,
          {
            headers: {
              'Content-type': 'application/json',
              Authorization: 'bearer ' + token.__raw
            }
          }
        )
        this.getNewHires()
      }
    },
    update (data) {
      this.isUpdate = true
      this.form.empNo = data.empNo
      this.form.name = data.name
      this.form.email = data.email
      this.form.category = data.category
      // this.form.categoryWithDeck = data.categoryWithDeck
      this.showFormModal = true
    },
    showModal () {
      this.showFormModal = true
    },
    closeModal () {
      this.form = {
        empNo: '',
        name: '',
        email: '',
        category: '',
        categoryWithDeck: []
      }
      this.showFormModal = false
    },
    async submitRecord () {
      const token = await this.$auth.getIdTokenClaims()
      this.$isLoading(true)

      this.form.creationDateTime = moment(new Date()).format(
        'MMMM DD YYYY, h:mm a'
      )
      if (!this.isUpdate) {
        // Check if email already exist
        const exist = (
          await axios.get(
          `https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/checkhire/${this.form.email}`,
          {
            headers: {
              authorization: 'bearer ' + token.__raw
            }
          }
          )
        ).data

        if (exist) {
          alert('Email is already in use.')
          this.$isLoading(false)
          return
        }

        await axios.post(
          'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/new-hires',
          this.form,
          {
            headers: {
              'Content-type': 'application/json',
              Authorization: 'bearer ' + token.__raw
            }
          }
        )
      } else {
        // console.log('put')
        await axios.put(
          'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/updatenewhire/' + this.form.empNo,
          this.form,
          {
            headers: {
              'Content-type': 'application/json',
              Authorization: 'bearer ' + token.__raw
            }
          }
        )
      }
      this.isUpdate = false
      this.showFormModal = false
      this.form = {
        empNo: '',
        name: '',
        email: '',
        category: '',
        categoryWithDeck: []
      }
      this.$isLoading(false)
      this.getNewHires()
    },
    async getNewHires () {
      const token = await this.$auth.getIdTokenClaims()
      this.$isLoading(true)
      this.newHires = (
        await axios.get(
          `https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/getnewhires/${this.paging.currentPage}/${this.paging.pageSize}/${this.paging.search}`,
          {
            headers: {
              authorization: 'bearer ' + token.__raw
            }
          }
        )
      ).data
      this.$isLoading(false)
    },
    exportData () {
      let table = 'tblNewHires'
      const name = 'newhires'
      const filename = 'newhires.xls'
      const uri = 'data:application/vnd.ms-excel;base64,'
      const template =
        '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><title></title><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--><meta http-equiv="content-type" content="text/plain; charset=UTF-8"/></head><body><table>{table}</table></body></html>'
      const base64 = function (s) {
        return window.btoa(decodeURIComponent(encodeURIComponent(s)))
      }
      const format = function (s, c) {
        return s.replace(/{(\w+)}/g, function (m, p) {
          return c[p]
        })
      }

      if (!table.nodeType) table = document.getElementById(table)
      var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML }

      var link = document.createElement('a')
      link.download = filename
      link.href = uri + base64(format(template, ctx))
      link.click()
    }
  },
  async mounted () {
    this.categories = (
      await axios.get('https://ninjamon-orientationapp.azurewebsites.net/api/getcategories')
    ).data

    this.getNewHires()
  }
}
</script>
