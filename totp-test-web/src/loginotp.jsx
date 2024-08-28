import { useState } from "react";
import { useNavigate } from "react-router-dom";

const LoginOTP = () => {
  const [emailAddress, setEmailAddress] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    const blog = { emailAddress, password };

    try {
      const response = await fetch('https://localhost:44363/api/accounts/loginotp', {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(blog)
      })
      if(response.status === 200) {
        let data = await response.json();
        console.log(data.message);
        if(data.isTwoFactor === true) {
          navigate('/twofactorotp', {state: {email:data.email, provider:data.provider}});
        } else {
          alert("2FA is not set up");
        }
      } else if( response.status === 401) {
        alert("Invalid username or password");
      } else {
        let data = await response.json();
        if(data !== null) {
          let errors = data.errors;
          if(errors !== null)
          {
            console.log(errors);
          }
        }
        alert("Unknown error");
        
      }
    } catch(err) {
      console.log(err);
      alert("Could not connect to server");
    }
    
    /*.then((response) => {
        return response.json();
    }).then((data) => {
        console.log(data);
        if(data.isTwoFactor === true){
            //navigate('Twofactor', {state:{email:data.emailAddress}});
            navigate('/twofactorotp', {state: {email:data.email, provider:data.provider}});
        }
    }).catch((error) => {
      console.log(error);
      alert("Could not connect to server");
    })*/
  }

  return (
    <div className="loginotp">
      <h2>Login OTP</h2>
      <form onSubmit={handleSubmit}>
        <label>Name:</label>
        <input 
          type="text" 
          required 
          value={emailAddress}
          onChange={(e) => setEmailAddress(e.target.value)}
        />
        <label>Password:</label>
        <input
          type="text"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        ></input>
        <button>Login</button>
      </form>
    </div>
  );
}
 
export default LoginOTP;