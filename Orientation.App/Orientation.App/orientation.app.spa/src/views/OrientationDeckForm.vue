<template>
  <div class="container" v-if="$auth.isAuthenticated" style="padding: 10px">
    <a
      href="javascript:void(0)"
      class="button is-floating is-medium"
      title="Add Orientation Deck"
      @click="showModal"
      v-if="selectedCategory"
    >
      <i class="fas fa-folder-plus"></i>
    </a>

    <div class="modal" :class="{ 'is-active': showPreview }">
      <div class="modal-background"></div>
      <div class="modal-content">
        <pdf :src="previewUrl"></pdf>
      </div>
      <button
        class="modal-close is-large"
        aria-label="close"
        @click="showPreview = !showPreview"
      ></button>
    </div>

    <div class="modal" :class="{ 'is-active': openForm }">
      <div class="modal-background"></div>
      <div class="modal-content">
        <div class="box">
          <form @submit.prevent="saveTabs">
            <div class="field">
              <label class="label">Name</label>
              <div class="control">
                <input
                  v-model="form.name"
                  class="input"
                  type="text"
                  placeholder="HR Orientation"
                  required
                />
              </div>
            </div>
            <div class="field">
              <label class="label">Description</label>
              <div class="control">
                <textarea
                  class="input"
                  rows="4"
                  required
                  v-model="form.description"
                  placeholder="Eg. Company Manuals, Brochures etc..."
                ></textarea>
              </div>
            </div>
            <div class="field">
              <label class="label">Files</label>
              <vue-dropzone
                id="fileUpload"
                ref="phunkeyDropzone"
                :options="dropzoneOptions"
                :use-custom-slot="true"
                @vdropzone-file-added="handleFileAdding"
              >
                <!-- <div class="dropzone-custom-content">
                <span class="m-form__help">
                  Valid File Types: (.jpg,.pdf,.tif,.zip,.txt,.xls,.doc,.jpeg,.tiff,.xlsx,.docx,.csv,.rtf,.png,.gif)
                </span>
                <h3 class="dropzone-custom-title">
                  Drag and drop to upload content!
                </h3>
                <div class="subtitle">
                  ...or click to select a file from your computer
                </div>
              </div> -->
              </vue-dropzone>
            </div>
            <div class="field">
              <div class="control">
                <button class="button is-primary" type="submit">Submit</button>
              </div>
            </div>
          </form>
        </div>
      </div>
      <button
        class="modal-close is-large"
        aria-label="close"
        @click="closeModal"
      ></button>
    </div>

    <div class="field">
      <label class="label">Category</label>
      <div class="control">
        <div class="select">
          <select v-model="selectedCategory">
            <option v-for="cat in categories" :key="cat.name">
              {{ cat.name }}
            </option>
          </select>
        </div>
      </div>
    </div>
    <div class="field" style="margin-top: 50px" v-if="selectedCategory">
      <h2 class="title">Orientation Deck</h2>
      <div class="table-container">
        <table class="table is-fullwidth is-striped is-hoverable">
          <thead>
            <tr>
              <th></th>
              <th>Name</th>
              <th>Description</th>
              <th>Files</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="tab in tabList" :key="tab.name">
              <td>
                <p class="buttons">
                  <button
                    class="button is-small"
                    @click="updateMode(tab, tab._id)"
                  >
                    <span class="icon is-small">
                      <i class="fas fa-edit"></i>
                    </span>
                  </button>
                  <button class="button is-small" @click="deleteTab(tab)">
                    <span class="icon is-small">
                      <i class="fas fa-trash"></i>
                    </span>
                  </button>
                </p>
              </td>
              <td>{{ tab.name }}</td>
              <td>{{ tab.description }}</td>
              <td>
                <a
                  href="javascript:void(0)"
                  v-for="tabFile in tab.files"
                  :key="tabFile.FileName"
                  style="padding-right: 10px"
                  :title="tabFile.FileName"
                  @click="openFile(tab.name, tabFile.FileName)"
                >
                  <i :class="getClassForFileType(tabFile.FileName)" />
                </a>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script>
import VueDropzone from 'vue2-dropzone'
import 'vue2-dropzone/dist/vue2Dropzone.min.css'
import axios from 'axios'
import pdf from 'vue-pdf'
export default {
  components: {
    VueDropzone,
    pdf
  },
  data () {
    return {
      categories: [],
      selectedCategory: null,
      openForm: false,
      isUpdate: false,
      form: {
        category: '',
        name: '',
        description: '',
        files: []
      },
      dropzoneOptions: {
        url: 'https://httpbin.org/post',
        thumbnailWidth: 150,
        addRemoveLinks: true,
        // acceptedFiles:
        //   '.jpg,.pdf,.tif,.zip,.txt,.xls,.doc,.jpeg,.tiff,.xlsx,.docx,.csv,.rtf,.png,.gif,.pptx',
        acceptedFiles:
          '.pdf',
        autoProcessQueue: false, // Fake upload to display progress,
        dictDefaultMessage: "<i class='fa fa-cloud-upload'></i>UPLOAD ME"
      },
      tabList: [],
      showPreview: false,
      previewUrl: ''
    }
  },
  watch: {
    selectedCategory: async function (val, oldVal) {
      if (val) {
        const token = await this.$auth.getIdTokenClaims()
        this.$isLoading(true)
        this.tabList = (
          await axios.get(
            'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/getbycategory/orientation-decks/' +
              val,
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
  computed: {
    asynctoken () {
      const token = this.$auth.getIdTokenClaims()
      return token.__raw
    }
  },
  methods: {
    async openFile (tab, fileName) {
      const token = await this.$auth.getIdTokenClaims()
      const file = (await axios.get(
        `https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/openfile/${this.selectedCategory}/${tab}/${fileName}`,
        {
          headers: {
            authorization: 'bearer ' + token.__raw
          }
        }
      )
      ).data
      // window.open(file.files[0].dataURL)

      var link = document.createElement('a')
      link.download = fileName
      link.href = file.files[0].dataURL
      link.click()
    },
    closeModal () {
      this.isUpdate = false
      this.openForm = false
      this.form = {
        category: '',
        name: '',
        description: '',
        files: []
      }
      this.$refs.phunkeyDropzone.removeAllFiles()
    },
    async deleteTab (file) {
      var r = confirm('Confirm Delete?')
      if (r === true) {
        const token = await this.$auth.getIdTokenClaims()
        await axios.post(
          'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/delete/orientation-decks/' +
            file.name,
          file,
          {
            headers: {
              'Content-type': 'application/json',
              Authorization: 'bearer ' + token.__raw
            }
          }
        )
        this.tabList = (
          await axios.get(
            'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/getbycategory/orientation-decks/' +
              this.selectedCategory,
            {
              headers: {
                authorization: 'bearer ' + token.__raw
              }
            }
          )
        ).data
      }
    },
    openPreviewModal (url) {
      this.previewUrl = url
      this.showPreview = true
    },
    updateMode (file, id) {
      this.isUpdate = true
      file.files.forEach((el) => {
        const manualFile = this.getFileFromDataUrl(el.dataURL, el.FileName)
        this.$refs.phunkeyDropzone.manuallyAddFile(manualFile, el.dataURL)
      })
      this.form.id = id
      this.form.category = this.selectedCategory
      this.form.name = file.name
      this.form.oldName = file.name
      this.form.description = file.description
      this.openForm = true
    },
    getClassForFileType (file) {
      const fileExtension = file.substring(
        file.lastIndexOf('.') + 1,
        file.length
      )
      let mimeType = ''
      switch (fileExtension) {
        case 'png':
        case 'jpg':
        case 'jpeg':
        case 'tiff':
        case 'gif':
          mimeType = 'btn m-btn m-btn--link fa fa-file-image'
          break
        case 'pdf':
          mimeType = 'btn m-btn m-btn--link fa fa-file-pdf'
          break
        case 'doc':
        case 'dot':
        case 'docx':
        case 'rtf':
          mimeType = 'btn m-btn m-btn--link far fa-file-word'
          break
        case 'xls':
        case 'xlt':
        case 'xla':
        case 'xlsx':
          mimeType = 'btn m-btn m-btn--link far fa-file-excel'
          break
        case 'zip':
          mimeType = 'btn m-btn m-btn--link far fa-file-archive'
          break
        case 'csv':
          mimeType = 'btn m-btn m-btn--link far fa-file-csv'
          break
        case 'txt':
        case 'text':
          mimeType = 'btn m-btn m-btn--link far fa-sticky-note'
          break
        case 'pptx':
          mimeType = 'btn m-btn m-btn--link fas fa-file-powerpoint'
      }
      return mimeType + ' is-large'
    },
    showModal () {
      this.openForm = true
    },
    handleFileAdding (file) {
      this.getDataURL(file)
    },
    async saveTabs () {
      if (this.form.files.length === 0) {
        alert('Please Attach a file')
        return
      }

      this.$isLoading(true)
      // CHeck if name is aleady in use
      const exist = this.tabList.find(el => el.name === this.form.name)
      if (exist) {
        this.$isLoading(false)
        alert('Deck Name already in use!')
        return
      }

      this.form.category = this.selectedCategory

      const token = await this.$auth.getIdTokenClaims()

      this.form.isComplete = false
      this.form.completionTime = ''
      if (!this.isUpdate) {
        await axios.post(
          'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/orientation-decks',
          this.form,
          {
            headers: {
              'Content-type': 'application/json',
              Authorization: 'bearer ' + token.__raw
            }
          }
        )
      } else {
        await axios.put(
          'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/orientation-decks/' +
            this.form.oldName,
          this.form,
          {
            headers: {
              'Content-type': 'application/json',
              Authorization: 'bearer ' + token.__raw
            }
          }
        )
      }

      this.$isLoading(true)
      this.tabList = (
        await axios.get(
          'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/getbycategory/orientation-decks/' +
            this.selectedCategory,
          {
            headers: {
              authorization: 'bearer ' + token.__raw
            }
          }
        )
      ).data

      this.form = {
        category: '',
        name: '',
        description: '',
        files: []
      }
      this.isUpdate = false
      this.$refs.phunkeyDropzone.removeAllFiles()
      this.openForm = false
      this.$isLoading(false)
    },
    getDataURL (file) {
      // eslint-disable-next-line no-undef
      var fileReader = new FileReader()
      var base64
      fileReader.onload = (fileLoadedEvent) => {
        base64 = fileLoadedEvent.target.result
        file.dataURL = base64

        if (file.accepted === false) {
          alert('File not accepted. Only pdf files are allowed at the moment.')

          this.$refs.phunkeyDropzone.removeFile(file)
          return
        }

        this.form.files.push({
          dataURL: file.dataURL,
          // Base64: file.dataURL.split(',')[1],
          FileName: file.name,
          Size: file.size,
          ContentType: file.type
        })
      }
      fileReader.readAsDataURL(file)

      // Checks every second for the dataURL.
      var checkIfReady = setInterval(() => {
        if (fileReader.result) {
          // USE the DATAURL
          clearInterval(checkIfReady)
        }

        // Stops checking after 10 seconds.
        setTimeout(() => {
          clearInterval(checkIfReady)
        }, 10000)
      }, 1000)
    },
    getFileFromDataUrl (dataUrl, fileName) {
      var arr = dataUrl.split(',')
      var mime = arr[0].match(/:(.*?);/)[1]
      var bstr = atob(arr[1])
      var n = bstr.length
      var u8arr = new Uint8Array(n)

      while (n--) {
        u8arr[n] = bstr.charCodeAt(n)
      }

      return new File([u8arr], fileName, { type: mime })
    }
  },
  async mounted () {
    this.categories = (
      await axios.get('https://ninjamon-orientationapp.azurewebsites.net/api/getcategories')
    ).data
  }
}
</script>
