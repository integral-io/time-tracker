import {Command, flags} from '@oclif/command'

export default class Config extends Command {
    
  static description = 'Configure the cli. Can set your name, email, projects and categories'

  static examples = [
    `$ itt config`,
  ]
  
  async run(): Promise<any> {
    const {args, flags} = this.parse(Config)
    throw new Error("Method not implemented.");
  }
}